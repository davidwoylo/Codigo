using CFC_Persistencia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace CFC_Negocio.Servicos
{
    public class BaseServico : PadraoBD
    {
        public BaseServico() { }
        public BaseServico(string classType) : base(classType) { }
        public BaseServico(DbContext ctx) : base(ctx) { }


        public E Salvar<E>(object dto, bool ignoreMapping = false) where E : class
        {
            if (dto is IEnumerable)
            {
                foreach (var item in dto as IEnumerable)
                {
                    Salvar<E>(item, ignoreMapping);
                };
                return null;
            }
            else
            {
                return Salvar(!ignoreMapping ? AutoMapper.Mapper.Map<E>(dto) as E : dto as E);
            }
        }

        public void Atualizar(object dto, string[] properties = null)
        {
            if (dto is IEnumerable)
            {
                foreach (var item in dto as IEnumerable)
                {
                    this.Atualizar(item, properties);
                };
            }
            else
            {
                base.Atualizar(AutoMapper.Mapper.Map(dto, dto.GetType(), entityType) as object, properties);
            }
        }
        public void Atualizar<E>(object dto, IEnumerable<string> properties = null) where E : class
        {
            if (dto is IEnumerable)
            {
                foreach (var item in dto as IEnumerable)
                {
                    this.Atualizar<E>(item, properties);
                };
            }
            else
            {
                base.Atualizar<E>(AutoMapper.Mapper.Map<E>(dto) as E, properties);
            }
        }

        public void Excluir<E>(IEnumerable<dynamic> dto) where E : class
        {
            dto.ToList().ForEach(obj => this.Excluir(obj.id));
        }
    }
}
