using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FunctionAppFiap
{
    public static class HttpStart
    {
        [FunctionName("HttpStart")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Extrair os dados do pedido do corpo da solicitação HTTP
            var pedido = await req.Content.ReadAsAsync<Pedido>();

            // Iniciar o processo de aprovação
            string instanceId = await starter.StartNewAsync(nameof(PedidoOrchestrator), pedido);

            log.LogInformation($"Iniciado processo de aprovação com ID {instanceId}");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
