using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FunctionAppFiap
{
    public static class PedidoOrchestrator
    {
        [FunctionName("PedidoOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var pedido = context.GetInput<Pedido>();

            // Primeira atividade: Processar aprova��o
            bool aprovado = await context.CallActivityAsync<bool>(nameof(AprovarPedido), pedido);

            // Segunda atividade: Processar pagamento
            bool pagamentoProcessado = await context.CallActivityAsync<bool>(nameof(ProcessarPagamento), (pedido.Numero, aprovado));

            // Terceira atividade: Processar envio
            string numeroRastreamento = await context.CallActivityAsync<string>(nameof(ProcessarEnvio), (pedido.Numero, pagamentoProcessado));

            // Quarta atividade: Notificar usu�rio
            await context.CallActivityAsync(nameof(NotificarUsuario), (pedido.Numero, aprovado, pagamentoProcessado, numeroRastreamento));
        }

        [FunctionName(nameof(AprovarPedido))]
        public static bool AprovarPedido([ActivityTrigger] Pedido pedido, ILogger log)
        {
            log.LogInformation($"Solicita��o de aprova��o para o pedido {pedido.Numero}.");

            // L�gica de aprova��o simulada (pode ser mais complexa na implementa��o real)
            return pedido.Valor <= 1000;
        }

        [FunctionName(nameof(NotificarUsuario))]
        public static void NotificarUsuario([ActivityTrigger] (int Numero, bool Aprovado) pedidoStatus)
        {
            // L�gica de notifica��o simulada
            if (pedidoStatus.Aprovado)
            {
                Console.WriteLine($"Pedido {pedidoStatus.Numero} aprovado. Notificar usu�rio.");
            }
            else
            {
                Console.WriteLine($"Pedido {pedidoStatus.Numero} n�o aprovado. Notificar usu�rio.");
            }
        }

        [FunctionName(nameof(ProcessarPagamento))]
        public static bool ProcessarPagamento([ActivityTrigger] (int Numero, bool Aprovado) pedidoStatus)
        {
            // L�gica simulada de processamento de pagamento
            if (pedidoStatus.Aprovado)
            {
                Console.WriteLine($"Pagamento do pedido {pedidoStatus.Numero} processado com sucesso.");
                return true;
            }

            Console.WriteLine($"Pagamento do pedido {pedidoStatus.Numero} n�o processado devido � n�o aprova��o.");
            return false;
        }

        [FunctionName(nameof(ProcessarEnvio))]
        public static string ProcessarEnvio([ActivityTrigger] (int Numero, bool PagamentoProcessado) pedidoStatus)
        {
            // L�gica simulada de processamento de envio
            if (pedidoStatus.PagamentoProcessado)
            {
                string numeroRastreamento = Guid.NewGuid().ToString();
                Console.WriteLine($"Pedido {pedidoStatus.Numero} enviado com n�mero de rastreamento {numeroRastreamento}.");
                return numeroRastreamento;
            }

            Console.WriteLine($"Pedido {pedidoStatus.Numero} n�o enviado devido a pagamento n�o processado.");
            return null;
        }

    }
}