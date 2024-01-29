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

            // Primeira atividade: Processar aprovação
            bool aprovado = await context.CallActivityAsync<bool>(nameof(AprovarPedido), pedido);

            // Segunda atividade: Processar pagamento
            bool pagamentoProcessado = await context.CallActivityAsync<bool>(nameof(ProcessarPagamento), (pedido.Numero, aprovado));

            // Terceira atividade: Processar envio
            string numeroRastreamento = await context.CallActivityAsync<string>(nameof(ProcessarEnvio), (pedido.Numero, pagamentoProcessado));

            // Quarta atividade: Notificar usuário
            await context.CallActivityAsync(nameof(NotificarUsuario), (pedido.Numero, aprovado, pagamentoProcessado, numeroRastreamento));
        }

        [FunctionName(nameof(AprovarPedido))]
        public static bool AprovarPedido([ActivityTrigger] Pedido pedido, ILogger log)
        {
            log.LogInformation($"Solicitação de aprovação para o pedido {pedido.Numero}.");

            // Lógica de aprovação simulada (pode ser mais complexa na implementação real)
            return pedido.Valor <= 1000;
        }

        [FunctionName(nameof(NotificarUsuario))]
        public static void NotificarUsuario([ActivityTrigger] (int Numero, bool Aprovado) pedidoStatus)
        {
            // Lógica de notificação simulada
            if (pedidoStatus.Aprovado)
            {
                Console.WriteLine($"Pedido {pedidoStatus.Numero} aprovado. Notificar usuário.");
            }
            else
            {
                Console.WriteLine($"Pedido {pedidoStatus.Numero} não aprovado. Notificar usuário.");
            }
        }

        [FunctionName(nameof(ProcessarPagamento))]
        public static bool ProcessarPagamento([ActivityTrigger] (int Numero, bool Aprovado) pedidoStatus)
        {
            // Lógica simulada de processamento de pagamento
            if (pedidoStatus.Aprovado)
            {
                Console.WriteLine($"Pagamento do pedido {pedidoStatus.Numero} processado com sucesso.");
                return true;
            }

            Console.WriteLine($"Pagamento do pedido {pedidoStatus.Numero} não processado devido à não aprovação.");
            return false;
        }

        [FunctionName(nameof(ProcessarEnvio))]
        public static string ProcessarEnvio([ActivityTrigger] (int Numero, bool PagamentoProcessado) pedidoStatus)
        {
            // Lógica simulada de processamento de envio
            if (pedidoStatus.PagamentoProcessado)
            {
                string numeroRastreamento = Guid.NewGuid().ToString();
                Console.WriteLine($"Pedido {pedidoStatus.Numero} enviado com número de rastreamento {numeroRastreamento}.");
                return numeroRastreamento;
            }

            Console.WriteLine($"Pedido {pedidoStatus.Numero} não enviado devido a pagamento não processado.");
            return null;
        }

    }
}