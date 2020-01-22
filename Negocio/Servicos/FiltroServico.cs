using CFC_Entidades;
using CFC_Negocio.DTO;
using CFC_Negocio.DTO.Filtros;
using CFC_Negocio.Exceptions;
using CFC_Negocio.Util;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CFC_Negocio.Servicos
{
    public class FiltroServico : ValidacaoServico
    {
        public FiltroServico() { }
        public FiltroServico(string classType) : base(classType) { }


        public IQueryable<E> Filtro<E>(UsuarioFiltroDTO dto)
            where E : Usuario
        {
            IQueryable<E> result = ObterTodos<E>();

            if (dto != null)
            {
                if (!string.IsNullOrEmpty(dto.query))
                {
                    DateTime data = dto.query.ResolveDate();
                    decimal numeralDec = dto.query.AsDecimal();

                    result = result.Where(x =>
                        x.Nome == dto.query ||
                        x.Endereco == dto.query ||
                        x.Email == dto.query 
                    );
                }
            }

            return result;
        }

    }
}
