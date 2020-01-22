using CFC_Entidades;
using CFC_Negocio.DTO;
using CFC_Negocio.Exceptions;
using CFC_Persistencia.Util;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CFC_Negocio.Servicos
{
    public class ValidacaoServico : BaseServico
    {
        public ValidacaoServico() { }
        public ValidacaoServico(string classType) : base(classType) { }
        public ValidacaoServico(DbContext ctx) : base(ctx) { }

        public virtual void PadraoValidarSalvar<E>(object dto)
            where E : class
        {
            var listOfFieldNames = dto.GetType().GetProperties().Select(f => f.Name).ToList();

            if (listOfFieldNames.Contains("nome") && ObterTodos<E>().WhereEquals("nome", (string)((dynamic)dto).nome.ToLower()).Any())
                throw new KnownException("Número de norma já cadastrado.");

            if (listOfFieldNames.Contains("num_norma") && ObterTodos<E>().WhereEquals("num_norma", (string)((dynamic)dto).num_norma.ToLower()).Any())
                throw new KnownException("Nome já cadastrado.");

        }
        public virtual void PadraoValidarAtualizar<E>(object dto)
            where E : class
        {
            var listOfFieldNames = dto.GetType().GetProperties().Select(f => f.Name).ToList();

            if (listOfFieldNames.Contains("num_norma") && ObterTodos<E>().WhereEquals("num_norma", (string)((dynamic)dto).nome.ToLower()).WhereNotEquals("id", (int)((dynamic)dto).id).Any())
                throw new KnownException("Número de norma já cadastrado.");

            if (listOfFieldNames.Contains("nome") && ObterTodos<E>().WhereEquals("nome", (string)((dynamic)dto).nome.ToLower()).WhereNotEquals("id", (int)((dynamic)dto).id).Any())
                throw new KnownException("Nome já cadastrado.");

        }



        public virtual void ValidarSalvar<E>(UsuarioDTO dto)
            where E : Usuario
        {
            if (ObterTodos<E>().Any(x => x.Nome == dto.Nome))
                throw new KnownException("Nome já cadastrado.");
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime ExtrairData(string data)
        {
            try
            {
                string regex = " GMT";
                var valor = Regex.Split(data, regex);
                var date = valor[0];
                DateTime myDate = DateTime.ParseExact(date, "ddd MMM dd yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                return myDate;
            }
            catch (Exception)
            {
                string regex = " GMT";
                var valor = Regex.Split(data, regex);
                var date = valor[0];
                DateTime myDate = DateTime.ParseExact(date, "ddd dd MMM yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                return myDate;
            }
        }
    }
}
