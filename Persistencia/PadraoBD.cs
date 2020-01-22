using CFC_Persistencia.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace CFC_Persistencia
{
    public class PadraoBD
    {
        public DbContext bd = new Context();
        public dynamic table;
        public IEnumerable<dynamic> model;
        public Type entityType;

        const int CommitCount = 1000; //N° de inserts juntos por chamada

        public PadraoBD() { }
        public PadraoBD(string classType) { model = table = bd.EntityOf(classType, out entityType); }
        public PadraoBD(DbContext ctx) { bd = ctx; }

        public void SetClass(string classType)
        {
            model = table = bd.EntityOf(classType, out entityType);
        }

        /// <summary>
        /// Obtem todos os dados de um objeto que só é conhecido em tempo de execução
        /// </summary>
        /// <param name="classType">Nome do tipo do objeto a ser gerado</param>
        /// <param name="includes">Dependencias desse objeto a serem incluídas.</param>
        /// <returns></returns>
        public virtual IQueryable<dynamic> ObterTodos(string classType = null, string[] includes = null)
        {
            if (!string.IsNullOrEmpty(classType))
                SetClass(classType);
            IQueryable<dynamic> result = model?.AsQueryable();
            if (includes != null)
                foreach (string i in includes)
                    result = result.Include(i);
            return result.NaoExcluidos();
        }

        /// <summary>
        /// Obtem o objeto a partir do nome da classe passada filtrando os dados pelo campo id
        /// </summary>
        /// <param name="id">Valor do id a ser pesquisado</param>
        /// <param name="classType">Nome da classe a ser gerada em tempo de execução</param>
        /// <param name="includes">Dependencias da classe</param>
        /// <returns name="object"></returns>
        public object Obter(int id, string classType = null, IEnumerable<string> includes = null)
        {
            var entities = classType.Split('.');
            includes = includes ?? entities.Where(x => x != entities.First());
            if (!string.IsNullOrEmpty(classType))
                SetClass(entities.FirstOrDefault());
            IQueryable<dynamic> result = model?.AsQueryable();
            MethodInfo whereEqualsMethodInfo = typeof(EFExtensions).GetMethod("WhereEquals", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(entityType);
            if (includes != null)
                foreach (string i in includes)
                    result = result.Include(i);
            result = (IQueryable<dynamic>)whereEqualsMethodInfo.Invoke(null, new object[] { result, "id", id });
            return result.FirstOrDefault();
        }

        public object ObterDinamico(string typeName, int id, IEnumerable<string> includes = null)
        {
            var entities = typeName.Split('.');
            includes = includes ?? entities.Where(x => x != entities.First());
            Type tableEntity = Type.GetType("CFC_Entidades." + entities.First() + ", CFC_Entidades");
            IQueryable<dynamic> dbObject = (IQueryable<dynamic>)
                                typeof(DbContext).GetMethod("Set", Type.EmptyTypes)
                                .MakeGenericMethod(tableEntity)
                                .Invoke(bd, null);
            IQueryable<dynamic> result = dbObject.AsQueryable();
            MethodInfo whereEqualsMethodInfo = typeof(EFExtensions).GetMethod("WhereEquals", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(tableEntity);
            if (includes != null)
                foreach (string i in includes)
                    result = result.Include(i);
            result = (IQueryable<dynamic>)whereEqualsMethodInfo.Invoke(null, new object[] { result, "id", id });
            return result.FirstOrDefault();
        }

        public void Salvar(object entity)
        {
            bd.Entry(entity).State = EntityState.Added;
            bd.SaveChanges();
        }
        public void Salvar(IEnumerable<object> entities)
        {
            Type tipo = entities.FirstOrDefault().GetType();
            bd.Set(tipo).AddRange(entities);
            bd.SaveChanges();
        }

        public void Atualizar(dynamic entity, string[] properties = null)
        {
            if (properties == null)
            {
                bd.Set(table).Attach(entity);
                bd.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                bd.Entry(entity).State = EntityState.Unchanged;
                foreach (var property in properties)
                {
                    if (property == "data_exclusao")
                    {
                        entity.data_exclusao = DateTime.Now;
                    }
                    bd.Entry(entity).Property(ExpressionHelper.GetExpressionText(property)).IsModified = true;
                }
            }
            bd.SaveChanges();
        }

        public void Excluir(int id)
        {
            dynamic entity = bd.Set(table).Find(id);
            bd.Entry(entity).State = EntityState.Deleted;
            bd.SaveChanges();
            bd.Dispose();
        }

        public virtual IQueryable<E> ObterTodos<E>(IEnumerable<string> includes = null, bool isNotTracking = false) where E : class
        {
            IQueryable<E> result = bd.Set<E>().AsQueryable();
            if (includes != null)
                foreach (string i in includes)
                    result = result.Include(i);
            if (isNotTracking) return result.NaoExcluidos().AsNoTracking();
            else return result.NaoExcluidos();
        }

        public virtual E Obter<E>(int id, IEnumerable<string> includes = null, bool isNotTracking = false) where E : class
        {
            IQueryable<E> result = bd.Set<E>().AsQueryable();
            if (includes != null)
                foreach (string i in includes)
                    result = result.Include(i);
            if (isNotTracking) return result.AsNoTracking().NaoExcluidos().WhereEquals("id", id).FirstOrDefault();
            else return result.NaoExcluidos().WhereEquals("id", id).FirstOrDefault();
        }

        public E Salvar<E>(E entidade) where E : class
        {
            if (entidade.GetType().GetProperties().Select(f => f.Name).Contains("data_inclusao"))
                ((dynamic)entidade).data_inclusao = DateTime.Now;
            bd.Entry(entidade).State = EntityState.Added;
            bd.SaveChanges();
            return entidade;
        }

        /// <summary>
        /// Persiste uma lista de entidades no banco de dados.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entities"></param>
        public void Salvar<E>(IEnumerable<E> entities)
        {
            Type tipo = entities.FirstOrDefault().GetType();
            bd.Set(tipo).AddRange(entities);
            bd.SaveChanges();
        }

        /// <summary>
        /// Persiste uma quantidade muito grande de dados, sendo realizada de 1000 em 1000 persistencias por vez.
        /// </summary>
        /// <typeparam name="E">Tipo de dado a ser persistido</typeparam>
        /// <param name="entities">Lista de dados a ser persistido</param>
        public void SalvarLista<E>(IEnumerable<E> entities) where E : class
        {
            int currentCount = 0;

            List<E> eList = entities.ToList();

            while (currentCount < entities.Count())
            {
                //Verifica o tamanho da inserção para não tentar inserir mais do que foi fornecido na lista
                int commitCount = CommitCount;
                if ((eList.Count - currentCount) < commitCount)
                    commitCount = eList.Count - currentCount;

                //Faz a varredura e monta o bloco atual para a inserção em conjunto
                for (int i = currentCount; i < (currentCount + commitCount); i++)
                    bd.Entry(eList[i]).State = EntityState.Added;

                //Efetua o commit do bloco atual
                bd.SaveChanges();

                //Desapega as entidades já inseridas para não sobrecarregar o contexto
                for (int i = currentCount; i < (currentCount + commitCount); i++)
                    bd.Entry(eList[i]).State = EntityState.Detached;

                currentCount += commitCount;
            }
        }

        public void Atualizar<E>(dynamic entidade, IEnumerable<string> properties = null) where E : class
        {
            if (properties == null)
            {
                bd.Set<E>().Attach(entidade);
                bd.Entry(entidade).State = EntityState.Modified;
            }
            else
            {
                bd.Set<E>().Attach(entidade);
                bd.Entry(entidade).State = EntityState.Unchanged;
                if (GetColunasBoletoValue<E>(entidade).Contains("id_situacao")) {
                    if (entidade.id_situacao == 4)
                    {
                        foreach (var property in properties)
                        {
                            if (property == "data_exclusao")
                            {
                                entidade.data_exclusao = DateTime.Now;
                            }
                            bd.Entry<E>(entidade).Property(ExpressionHelper.GetExpressionText(property)).IsModified = true;
                        }
                    }
                    else
                    {
                        foreach (var property in properties)
                        {
                            if (property == "data_exclusao")
                            {
                                entidade.data_exclusao = DateTime.Now;
                            }
                            if (property == "id_situacao")
                            {
                                entidade.id_situacao = 1;
                            }
                            bd.Entry<E>(entidade).Property(ExpressionHelper.GetExpressionText(property)).IsModified = true;
                        }
                    }
                } else if (GetColunasBoletoValue<E>(entidade).Contains("id_situacao_boleto")) {
                    foreach (var property in properties)
                    {
                        if (property == "data_exclusao")
                        {
                            entidade.data_exclusao = DateTime.Now;
                        }
                        bd.Entry<E>(entidade).Property(ExpressionHelper.GetExpressionText(property)).IsModified = true;
                    }
                }else{
                    foreach (var property in properties)
                    {
                        if (property == "data_exclusao")
                        {
                            entidade.data_exclusao = DateTime.Now;
                        }
                        bd.Entry<E>(entidade).Property(ExpressionHelper.GetExpressionText(property)).IsModified = true;
                    }
                }


            }
            bd.SaveChanges();
        }

        public void Excluir<E>(int id, bool excluirFisicamente = false, bool inativar = false) where E : class
        {
            if (excluirFisicamente)
            {
                bd.Set<E>().Remove(bd.Set<E>().Find(id));
                bd.SaveChanges();
            }
            else if(inativar)
            {
                Atualizar<E>(bd.Set<E>().Find(id), new string[] { "data_exclusao", "id_situacao" });
            }
            else
            {
                Atualizar<E>(bd.Set<E>().Find(id), new string[] { "data_exclusao" });
            }
        }

        public void Excluir<E>(IQueryable<E> entityList, bool excluirFisicamente = false) where E : class
        {
            if (excluirFisicamente)
            {
                bd.Set<E>().RemoveRange(entityList);
                bd.SaveChanges();
            }
            else
            {
                foreach (E item in entityList)
                {
                    Atualizar<E>(item, new string[] { "data_exclusao" });
                };
            }
        }

        public List<string> GetColunasBoletoValue<E>(dynamic entidade)
        {
            var rawQuery = bd.Database.SqlQuery<string>("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + entidade.GetType().Name + "';");
            var task = rawQuery.ToListAsync();
            var nextVal = task.Result;

            return nextVal;
        }
    }
}
