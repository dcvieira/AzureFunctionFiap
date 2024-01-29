using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAppFiap
{
    public class Pedido
    {
        public int Numero { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public bool Aprovado { get; set; }
    }
}
