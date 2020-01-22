using CFC_Negocio.DTO.Ext;
using CFC_Negocio.Servicos;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static CFC_Negocio.Util.Refl;

namespace CFC_Negocio.Util
{

    public static class Extensions
    {
        public enum TipoArquivo
        {
            Anexo = 1,
            Remessa = 2,
            Alvara = 3,
            Certidao = 4
        }
        static readonly BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static PaginacaoDTO<R> AsPaginado<R>(this IQueryable<dynamic> lista, PaginacaoConfigDTO config)
        {
            return new PaginacaoDTO<R>(lista, config);
        }
        public static PaginacaoArrecadacaoDTO<R> AsPaginadoArrecadacao<R>(this IQueryable<dynamic> lista, PaginacaoConfigDTO config)
        {
            return new PaginacaoArrecadacaoDTO<R>(lista, config);
        }
        public static string AsBase64(this Stream stream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }
            return Convert.ToBase64String(bytes);
            //return new MemoryStream(Encoding.UTF8.GetBytes(base64));
        }
        public static string AsBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
            //return new MemoryStream(Encoding.UTF8.GetBytes(base64));
        }
        public static R As<R>(this object obj)
        {
            return AutoMapper.Mapper.Map<R>(obj);
        }
        public static List<R> AsList<R>(this IQueryable<dynamic> lista)
        {
            return AutoMapper.Mapper.Map<List<R>>(lista?.ToList());
        }
        public static List<R> AsList<R>(this IEnumerable<dynamic> lista)
        {
            return AutoMapper.Mapper.Map<List<R>>(lista?.ToList());
        }
        public static DateTime ResolveDate(this string filtro, string format = null)
        {
            DateTime retorno = new DateTime();
            if (string.IsNullOrEmpty(format))
            {
                switch (filtro.Length)
                {
                    case 2:
                        format = "MM";
                        break;
                    case 4:
                        format = "yyyy";
                        break;
                    case 5:
                        format = "dd/MM";
                        break;
                    case 7:
                        format = "MM/yyyy";
                        break;
                    case 10:
                        format = "dd/MM/yyyy";
                        break;
                }
            }
            DateTime.TryParseExact(filtro, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out retorno);
            return retorno;
        }
        public static int AsInt(this string filtro)
        {
            int.TryParse(filtro, out int retorno);
            return retorno;
        }
        public static decimal AsDecimal(this string filtro)
        {
            decimal.TryParse(filtro, out decimal retorno);
            return retorno;
        }
        public static string AsCamelCase(this string str)
        {
            return Regex.Replace(str, @"(?<!_|^)([A-Z])", "_$1").ToLower();
        }
        public static string GetProject(string str)
        {
            return Regex.Replace(str, @"^([^.]*).*", "$1");
        }
        public static string GetFunction(string str, string[] ignoredStrings = null)
        {
            ignoredStrings = ignoredStrings ?? new string[] { "combo" };
            return Regex.Match(str, $@"(?:\b(?!{string.Join("|", ignoredStrings)})(?!\/)\b)([^\/]*)").ToString();
        }
        public static E GetInstance<E>()
        {
            return (E)Activator.CreateInstance(typeof(E));
        }
        public static dynamic GetInstance(string type, bool camelCase = true, string referencedProject = "CFC_Entidades")
        {
            string fullName = "";
            if (camelCase)
            {
                fullName = $"{referencedProject}.{AsCamelCase(type)}, {GetProject(referencedProject)}";
            }
            else
            {
                fullName = $"{referencedProject}.{type}, {GetProject(referencedProject)}";
            }

            return Activator.CreateInstance(Type.GetType(fullName));
        }
        /// <summary>
        /// Método para salvar o arquivo em um diretório físico e padronizar a entidade para persistencia no banco de dados.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="listaAnexos"></param>
        /// <param name="chave"></param>
        /// <param name="tipoArquivo"></param>
        /// <param name="listaExclusao"></param>
        /// <returns></returns>
        public static List<E> StoreFiles<E, D>(this List<D> listaAnexos, string chave, TipoArquivo tipoArquivo, List<string> listaExclusao = null)
        {
            DateTime dataInclusao = DateTime.Now;
            string directory;
            switch (tipoArquivo)
            {
                case TipoArquivo.Anexo:
                    directory = Path.Combine(ConfigurationManager.AppSettings["uploadPath"], "Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Anexo.ToString(), dataInclusao.ToString("yyyy-MM"), chave);
                    break;
                case TipoArquivo.Remessa:
                    directory = Path.Combine(ConfigurationManager.AppSettings["uploadPath"], "Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Remessa.ToString(), dataInclusao.ToString("yyyy-MM"), chave);
                    break;
                case TipoArquivo.Alvara:
                    directory = Path.Combine(ConfigurationManager.AppSettings["uploadPath"], "Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Alvara.ToString(), dataInclusao.ToString("yyyy-MM"), chave);
                    break;
                case TipoArquivo.Certidao:
                    directory = Path.Combine(ConfigurationManager.AppSettings["uploadPath"], "Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Certidao.ToString(), dataInclusao.ToString("yyyy-MM"), chave);
                    break;
                default:
                    directory = Path.Combine(ConfigurationManager.AppSettings["uploadPath"], "Uploads", ConfigurationManager.AppSettings["sistema"], "Uploads", dataInclusao.ToString("yyyy-MM"), chave);
                    break;
            }

            List<E> anexos = new List<E>();

            listaAnexos.ForEach(file =>
            {
                if (((dynamic)file).id == 0)
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    };

                    GroupCollection fileInfo = Regex.Match(((dynamic)file).nome_upload, ":(.*);.*,(.*)").Groups;

                    string mimeType = fileInfo[1].ToString();
                    RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
                    string ext = key != null ? key.GetValue("Extension", null)?.ToString() : throw new Exception("Erro ao obter a extensão do arquivo.");
                    string fileName = Guid.NewGuid().ToString("D") + ext;

                    string filePath = Path.Combine(directory, fileName);

                    File.WriteAllBytes(filePath, Convert.FromBase64String(fileInfo[2].ToString()));

                    dynamic arquivo = file.As<E>();

                    arquivo.arquivo_anexo = Convert.FromBase64String(fileInfo[2].ToString());
                    arquivo.nome_upload = fileName;
                    arquivo.extensao = ext;
                    arquivo.data_inclusao = dataInclusao;

                    anexos.Add(arquivo);
                }
                if (listaExclusao != null)
                    listaExclusao.ForEach(arquivo => File.Delete(Path.Combine(directory, arquivo)));
            });

            return anexos;
        }
        /// <summary>
        /// Recupera os arquivos da entidade attach
        /// </summary>
        /// <param name="attach">Entidade usada para recuperar os arquivos</param>
        /// <param name="key">Valor chave usado na recuperação dos arquivos</param>
        /// <returns></returns>
        public static string RetrieveFiles(this object attach, string key)
        {
            var keyAttach = attach.getPropertyByName(key);
            string filePath = "";
            switch ((int)((dynamic)attach).tipo_documento)
            {
                case (int)TipoArquivo.Anexo:
                    filePath = Path.Combine("C:\\Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Anexo.ToString(), ((dynamic)attach).data_inclusao.ToString("yyyy-MM"), keyAttach.ToString(), ((dynamic)attach).nome_upload);
                    break;
                case (int)TipoArquivo.Remessa:
                    filePath = Path.Combine("C:\\Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Remessa.ToString(), ((dynamic)attach).data_inclusao.ToString("yyyy-MM"), keyAttach.ToString(), ((dynamic)attach).nome_upload);
                    break;
                case (int)TipoArquivo.Certidao:
                    filePath = Path.Combine("C:\\Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Certidao.ToString(), ((dynamic)attach).data_inclusao.ToString("yyyy-MM"), keyAttach.ToString(), ((dynamic)attach).nome_upload);
                    break;
                case (int)TipoArquivo.Alvara:
                    filePath = Path.Combine("C:\\Uploads", ConfigurationManager.AppSettings["sistema"], TipoArquivo.Alvara.ToString(), ((dynamic)attach).data_inclusao.ToString("yyyy-MM"), keyAttach.ToString(), ((dynamic)attach).nome_upload);
                    break;
                default:
                    filePath = Path.Combine("C:\\Uploads", ConfigurationManager.AppSettings["sistema"], "Uploads", ((dynamic)attach).data_inclusao.ToString("yyyy-MM"), keyAttach.ToString(), ((dynamic)attach).nome_upload);
                    break;
            }
            return File.ReadAllBytes(filePath).AsBase64();
        }
        /// <summary>
        /// Percorre uma entidade dinâmica e retorna o valor de um atributo genérico, podendo percorrer entidades filhas que compõem o atributo.
        /// </summary>
        /// <param name="attach">Entidade dinâmica a ser percorrida</param>
        /// <param name="property">Caminho do atributo a ser percorrido</param>
        /// <returns></returns>
        public static dynamic getPropertyByName(this object attach, string property)
        {
            if (attach != null)
            {
                var propList = property.Split('.');
                try
                {
                    var valueObject = attach.GetType().GetProperty(propList[0], flags).GetValue(attach);

                    if (valueObject == null)
                        return "";
                    else if (propList.Length == 1)
                        return valueObject;

                    return valueObject.getPropertyByName(string.Join(".", propList.Skip(1)));
                }
                catch (Exception)
                {
                    return null;
                }
                
            }
            return null;
        }
        public static string GetFuncAcao(string funcionalidade, string action)
        {
            funcionalidade = GetFunction(funcionalidade).AsCamelCase().ToUpper();

            switch (action)
            {
                case "GET":
                    return $"{funcionalidade}_CONSULTAR";
                case "POST":
                    return $"{funcionalidade}_INSERIR";
                case "PUT":
                    return $"{funcionalidade}_EDITAR";
                case "DELETE":
                    return $"{funcionalidade}_REMOVER";
                default:
                    return "";
            }
        }
        public static MethodInfo GetMethod(this Type t, string name, params Type[] parameters)
        {
            foreach (var method in t.GetMethods())
            {
                // easiest case: the name doesn't match!
                if (method.Name != name)
                    continue;
                // set a flag here, which will eventually be false if the method isn't a match.
                var correct = true;
                if (method.IsGenericMethodDefinition)
                {
                    // map the "private" Type objects which are the type parameters to
                    // my public "Tx" classes...
                    var d = new Dictionary<Type, Type>();
                    var args = method.GetGenericArguments();
                    if (args.Length >= 1)
                        d[typeof(T1)] = args[0];
                    if (args.Length >= 2)
                        d[typeof(T2)] = args[1];
                    if (args.Length >= 3)
                        d[typeof(T3)] = args[2];
                    if (args.Length >= 4)
                        d[typeof(T3)] = args[3];
                    if (args.Length > 4)
                        throw new NotSupportedException("Too many type parameters.");

                    var p = method.GetParameters();
                    for (var i = 0; i < p.Length; i++)
                    {
                        // Find the Refl.TX classes and replace them with the 
                        // actual type parameters.
                        var pt = Substitute(parameters[i], d);
                        // Then it's a simple equality check on two Type instances.
                        if (pt != p[i].ParameterType)
                        {
                            correct = false;
                            break;
                        }
                    }
                    if (correct)
                        return method;
                }
                else
                {
                    var p = method.GetParameters();
                    for (var i = 0; i < p.Length; i++)
                    {
                        var pt = parameters[i];
                        if (pt != p[i].ParameterType)
                        {
                            correct = false;
                            break;
                        }
                    }
                    if (correct)
                        return method;
                }
            }
            return null;
        }
        private static Type Substitute(Type t, IDictionary<Type, Type> env)
        {
            // We only really do something if the type 
            // passed in is a (constructed) generic type.
            if (t.IsGenericType)
            {
                var targs = t.GetGenericArguments();
                for (int i = 0; i < targs.Length; i++)
                    targs[i] = Substitute(targs[i], env); // recursive call
                t = t.GetGenericTypeDefinition();
                t = t.MakeGenericType(targs);
            }
            // see if the type is in the environment and sub if it is.
            return env.ContainsKey(t) ? env[t] : t;
        }
        /// <summary>
        /// Realiza a deleção lógica de um objeto e atualiza os atributos passados pelo usuário.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="obj">Objeto principal</param>
        /// <param name="partial">Lista de atributos parciais a ser atualizados, caso o parâmetro deletLogic não seja passado, não é necessário adicionar o campo data_exclusao pois o mesmo será adicionado a lista.</param>
        /// <param name="listAttributes">Lista de atributos a ser atualizado</param>
        /// <param name="listSetValues">Lista dos valores a ser usado para atualizar os campos da lista de atributos</param>
        /// <param name="deleteLogic">Campo para verificar se ocorrerá a deleção lógica, valor default = true</param>
        public static void UpdateObject<E>(this object obj, FiltroServico servico, IEnumerable<string> partial = null, IEnumerable<string> listAttributes = null, IEnumerable<Object> listSetValues = null, bool deleteLogic = true)
        where E : class
        {
            if (deleteLogic) partial = partial == null ? new string[] { "data_exclusao" } : partial.Contains("data_exclusao") ? partial : partial.Concat(new string[] { "data_exclusao" });
            if ((int)obj.getPropertyByName("id") > 0)
            {
                servico.Atualizar<E>(obj.SetValues(listAttributes, listSetValues), partial);
            }
        }
        /// <summary>
        /// Realiza uma deleção lógica de uma lista de objetos L, atualizando também atributos que são passados pela lista listAttributes setando os valores através dos indexs correspondentes da lista listSetValues
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="L"></typeparam>
        /// <param name="lista"></param>
        /// <param name="partial"></param>
        /// <param name="listAttributes"></param>
        /// <param name="listSetValues"></param>
        public static void DeleteLogicList<E, L>(this List<L> lista, FiltroServico servico, string[] partial = null, IEnumerable<string> listAttributes = null, IEnumerable<Object> listSetValues = null)
        where E : class
        where L : class
        {
            lista.ForEach(delegate (L obj)
            {
                obj.UpdateObject<E>(servico, partial, listAttributes, listSetValues);
            });
        }
        /// <summary>
        /// Atualiza uma lista de valores passada pelo usuário, podendo ou não excluir os dados logicamente.
        /// </summary>
        /// <typeparam name="E">Tipo da entidade do banco de dados que vai ser atualizada</typeparam>
        /// <typeparam name="L">Tipo da entidade DTO a ser atualizada</typeparam>
        /// <param name="lista">Lista de entidade DTO</param>
        /// <param name="keyComparer">Chave para comparação da lista</param>
        /// <param name="valueComparer">Valor usado para comparação na lista</param>
        /// <param name="listAttributes">Lista de atributos a ser atualizado</param>
        /// <param name="listSetValues">Lista de valores usados para atualizar os campos da lista de atributos.</param>
        public static List<L> UpdateList<E, L>(this List<L> lista, FiltroServico servico, string keyType, Object valueComparer, bool deleteLogic = true, string[] listAttributes = null, Object[] listSetValues = null)
        where E : class
        where L : class
        {
            if (lista != null)
            {
                List<int> listId = lista.Where(x => (int)x.GetType().GetProperty("id").GetValue(x) > 0).Select(y => (int)y.GetType().GetProperty("id").GetValue(y)).ToList();
                IEnumerable<E> listaDelete = servico.ObterTodos<E>().PropertyEquals(keyType, valueComparer).ToList().Where(x => !listId.Contains((int)x.GetType().GetProperty("id").GetValue(x)));
                lista.RemoveAll(x => (int)x.GetType().GetProperty("id").GetValue(x) > 0);
                if (deleteLogic)
                {
                    listaDelete.ToList().DeleteLogicList<E, E>(servico, null, listAttributes, listSetValues);
                }
                else
                {
                    servico.Excluir(listaDelete.AsQueryable(), true);
                }
                lista.ForEach(delegate (L obj)
                {
                    obj.SetValues(listAttributes, listSetValues);
                });
                servico.Salvar<E>(lista);
            }
            return new List<L>();
        }
        /// <summary>
        /// Passa os valores recebidos de uma lista para uma entidade dinâmica
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="listAttributes">Lista de atributos a ser atualizados</param>
        /// <param name="listSetValues">Lista de valores a ser usado para atualizar os campos passados na lista de atributos</param>
        /// <returns></returns>
        public static dynamic SetValues(this object obj, IEnumerable<string> listAttributes, IEnumerable<Object> listSetValues)
        {
            if (listAttributes != null)
            {
                for (int i = 0; i < listAttributes.Count(); i++)
                {
                    obj.GetType().GetProperty(listAttributes.ElementAt(i)).SetValue(obj, listSetValues == null || listSetValues.Count() <= i ? null : listSetValues.ElementAt(i));
                }
            }
            return obj;
        }
        /// <summary>
        /// Cria uma expressão lambda que compara se os valores passados pelo usuário são iguais.
        /// </summary>
        /// <typeparam name="TItem">Tipo do item a ser comparado</typeparam>
        /// <typeparam name="TValue">Tipo do valor usado na comparação</typeparam>
        /// <param name="property">nome do campo a ser usado na comparação</param>
        /// <param name="value">valor a ser usado na comparação</param>
        /// <returns></returns>
        public static IQueryable<TItem> PropertyEquals<TItem, TValue>(this IQueryable<TItem> query, string keyType, TValue value)
        {
            PropertyInfo property = typeof(TItem).GetProperty(keyType);
            var param = Expression.Parameter(typeof(TItem));
            var body = Expression.Equal(Expression.Property(param, property), Expression.Constant(value));
            var predicate = Expression.Lambda<Func<TItem, bool>>(body, param);
            return query.Where(predicate);
        }
        
        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        /// <summary>
        /// Gera um guid único para a entidade passada comparando no banco se esse guid já existe.. caso exista o método é re-executado.
        /// </summary>
        /// <typeparam name="UC">tipo do objeto a ser usado na comparação</typeparam>
        /// <param name="obj">objeto usado para pegar o valor chave</param>
        /// <param name="keyValue">Campo chave para geração do guid</param>
        /// <param name="fieldComparer">Campo usado para comparar se a chave já foi cadastrada no banco</param>
        /// <param name="maxLength">Tamanho máximo para o retorno da guid valor máximo = 20, valor default = 20.</param>
        /// <param name="minLentgh">Tamanho mínimo para o retorno da guid valor máximo = 20, valor default = 0.</param>
        /// <returns></returns>
        public static string GenerateUniqueCode<UC>(this object obj, FiltroServico servico, string fieldComparer, int maxLength = 20)
            where UC : class
        {
            string code = Guid.NewGuid().ToString("N").Substring(0, maxLength).ToUpper();


            if (servico.ObterTodos<UC>().PropertyEquals(fieldComparer, code).Count() < 1)
            {
                return code;
            }
            else
            {
                return obj.GenerateUniqueCode<UC>(servico, fieldComparer, maxLength);
            }
        }
        /// <summary>
        /// Gera um relatório de acordo com os dados passados nos parâmetros. 
        /// </summary>
        /// <typeparam name="LR"></typeparam>
        /// <param name="listReport"></param>
        /// <param name="DataSetName"></param>
        /// <param name="pathReport"></param>
        /// <returns></returns>
        //public static RetornoRelatorio GenerateReport<LR>(this List<LR> listReport, string DataSetName, string namePathReport)
        //    where LR : class
        //{
        //    var report = new Microsoft.Reporting.WebForms.LocalReport();
        //    string link = System.Web.Hosting.HostingEnvironment.MapPath("~/");
        //    //report.ReportPath = link.Replace("CFC_Controlador\\", "") + "CFC_Negocio\\Relatorio\\" + namePathReport + ".rdlc";
        //    report.ReportPath = link + ConfigurationManager.AppSettings["reportPath"] + namePathReport + ".rdlc";
        //    report.DataSources.Add(new Microsoft.Reporting.WebForms.ReportDataSource(DataSetName, listReport));

        //    report.Refresh();

        //    string mimeType = "";
        //    string encoding = "";
        //    string filenameExtension = "";
        //    string[] streams = null;
        //    Microsoft.Reporting.WebForms.Warning[] warnings = null;
        //    byte[] bytes = report.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streams, out warnings);
        //    return new RetornoRelatorio
        //    {
        //        report = bytes,
        //        mimeType = mimeType
        //    };
        //}
    }
    public static class Refl
    {
        public sealed class T1 { }
        public sealed class T2 { }
        public sealed class T3 { }
        public sealed class T4 { }
        // ... more, if you so desire.
    }
}