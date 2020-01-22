using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CFC_Entidades;
using System.Reflection;
using System.Data.Entity;

namespace CFC_Persistencia
{
    public class Context : TesteBD
    {
        public Context()
        {
            //Desabilita detecção de alterações no db, acelera a inserção e atualização consideravelmente.
            Configuration.AutoDetectChangesEnabled = false;
            //Desabilita a criação de proxy, evitando erros de referencia circular.
            Configuration.ProxyCreationEnabled = false;
        }
    }
}
