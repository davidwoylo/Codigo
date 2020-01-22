using System;
using System.Collections.Generic;
using System.Linq;
using CFC_Negocio.Util;
using System.Linq.Dynamic;
using CFC_Negocio.Servicos;

namespace CFC_Negocio.DTO.Ext
{
    public class PaginacaoDTO<R>
    {
        private IQueryable<dynamic> listaObjeto;
        private PaginacaoConfigDTO config;

        public PaginacaoDTO(IQueryable<dynamic> listaObjeto, PaginacaoConfigDTO config)
        {
            this.listaObjeto = listaObjeto;
            this.config = config;
        }
        
        public List<R> content
        {
            get
            {
                try
                {
                    return listaObjeto.Count() != 0 ? AutoMapper.Mapper.Map<List<R>>(listaObjeto.OrderBy(string.Format("{0} {1}", string.IsNullOrEmpty(config?.sort) ? "id" : config?.sort, string.IsNullOrEmpty(config?.order) ? "desc" : config?.order)).Skip(number * numbersOfElements).Take(numbersOfElements).ToList()) : null;
                }
                catch (Exception ex)
                {
                    EnvioEmail email = new EnvioEmail();
                    email.EnviarEmail($"{ex.Message}", ex.GetType().ToString(), ex.Source?.ToString(), ex.StackTrace?.ToString(), ex.Data?.ToString(), ex.TargetSite?.ToString());
                    throw ex;
                }
                
            }
        }

        public int? totalElements
        {
            get
            {
                return listaObjeto.Count() != 0 ? listaObjeto.Count() : 0;
            }
        }

        public int totalPages
        {
            get
            {
                return listaObjeto.Count() != 0 ? (int)Math.Ceiling(listaObjeto.Count() / (decimal)(config?.size ?? 1)) : 0;
            }
        }

        public int numbersOfElements
        {
            get
            {
                return listaObjeto.Count() != 0 ? config?.size ?? 10 : 0;
            }
        }

        public int number
        {
            get
            {
                return config?.page ?? 0;
            }
        }

        public object sort
        {
            get
            {
                return config?.sort;
            }
        }

        public object order
        {
            get
            {
                return config?.order;
            }
        }

        public bool first
        {
            get
            {
                return false;
            }
        }

        public bool last
        {
            get
            {
                return false;
            }
        }
    }



    public class PaginacaoRegistroDTO<R>
    {

        public List<R> content { get; set; }

        public int? totalElements { get; set; }

        public int totalPages { get; set; }

        public int numbersOfElements { get; set; }
    }

    public class PaginacaoArrecadacaoDTO<R>
    {
        private IQueryable<dynamic> listaObjeto;
        private PaginacaoConfigDTO config;

        public PaginacaoArrecadacaoDTO(IQueryable<dynamic> listaObjeto, PaginacaoConfigDTO config)
        {
            this.listaObjeto = listaObjeto;
            this.config = config;
        }

        public List<R> content{
            get
            {
                return listaObjeto.Count() != 0 ? AutoMapper.Mapper.Map<List<R>>(listaObjeto.OrderBy(string.Format("{0} {1}", string.IsNullOrEmpty(config?.sort) ? "id" : config?.sort, string.IsNullOrEmpty(config?.order) ? "asc" : config?.order)).Skip(number * numbersOfElements).Take(numbersOfElements).ToList()) : null;
            }
        }

        public int? totalElements
        {
            get
            {
                return listaObjeto.Count() != 0 ? listaObjeto.Count() : 0;
            }
        }

        public int totalPages
        {
            get
            {
                return listaObjeto.Count() != 0 ? (int)Math.Ceiling(listaObjeto.Count() / (decimal)(config?.size ?? 1)) : 0;
            }
        }

        public int numbersOfElements
        {
            get
            {
                return listaObjeto.Count() != 0 ? config?.size ?? 10 : 0;
            }
        }

        public int number
        {
            get
            {
                return config?.page ?? 0;
            }
        }

        public object sort
        {
            get
            {
                return config?.sort;
            }
        }

        public object order
        {
            get
            {
                return config?.order;
            }
        }

        public bool first
        {
            get
            {
                return false;
            }
        }

        public bool last
        {
            get
            {
                return false;
            }
        }
    }
}
