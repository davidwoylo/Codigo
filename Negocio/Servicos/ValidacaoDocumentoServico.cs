using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CFC_Negocio.Servicos
{
    public class ValidacaoDocumentoServico
    {
        /// <summary>
        /// Método único de validação do CPF e CNPJ, o método recebe o valor a ser validado realizar o replace dos campos não numéricos caso houver.
        /// Em seguida realiza a validação da string passada verificando se atende ao padrão definido pela Receita Federal do Brasil.
        /// </summary>
        /// <param name="cpfcnpj"></param>
        /// <param name="field"></param>
        /// <returns type=string></returns>
        public bool ValidateCPFCNPJ(string cpfcnpj, string field = "")
        {
            if (string.IsNullOrEmpty(cpfcnpj)) return false;
            int[] d = new int[14];
            int[] v = new int[2];
            int j, i, soma;
            string Sequencia, numeroDocumento;
            numeroDocumento = Regex.Replace(cpfcnpj, @"[^\d]", string.Empty);

            if (new string(numeroDocumento[0], numeroDocumento.Length) == numeroDocumento) return false;
            switch (numeroDocumento.Length)
            {
                case 11:
                    for (i = 0; i <= 10; i++) d[i] = Convert.ToInt32(numeroDocumento.Substring(i, 1));
                    for (i = 0; i <= 1; i++)
                    {
                        soma = 0;
                        for (j = 0; j <= 8 + i; j++) soma += d[j] * (10 + i - j);

                        v[i] = (soma * 10) % 11;
                        if (v[i] == 10) v[i] = 0;
                    }
                    if (v[0] == d[9] || v[1] == d[10]) { return true; }
                    else { return false; }
                    break;
                case 14:
                    //Sequencia de valor para a validacao do CNPJ
                    Sequencia = "6543298765432";
                    for (i = 0; i <= 13; i++) d[i] = Convert.ToInt32(numeroDocumento.Substring(i, 1));
                    for (i = 0; i <= 1; i++)
                    {
                        soma = 0;
                        for (j = 0; j <= 11 + i; j++)
                            soma += d[j] * Convert.ToInt32(Sequencia.Substring(j + 1 - i, 1));

                        v[i] = (soma * 10) % 11;
                        if (v[i] == 10) v[i] = 0;
                    }
                    if (v[0] == d[12] || v[1] == d[13]) { return true; }
                    else { return false; }
                    break;
                default:
                    return false;
                    break;
            }
        }

        public string validaTituloEleitoral(string titulo)
        {
            if (string.IsNullOrEmpty(titulo)) return "Campo Título eleitoral é obrigatório";
            int[] d = new int[12];
            int dv1 = 0;
            int dv2 = 0;
            int j, i, somaDv1 = 0, somaDv2 = 0;
            string Sequencia, numeroDocumento;
            numeroDocumento = Regex.Replace(titulo, @"[^\d]", string.Empty);

            if (new string(numeroDocumento[0], numeroDocumento.Length) == numeroDocumento) return "Título eleitoral inválido.";
            for (i = 0; i <= 11; i++) d[i] = Convert.ToInt32(numeroDocumento.Substring(i, 1));

            for (j = 0; j <= 7; j++) somaDv1 += d[j] * (2 + j);
            dv1 = somaDv1 % 11;

            somaDv2 = (d[8] * 7) + (d[9] * 8) + (dv1 * 9);
            dv2 = somaDv2 % 11;

            if (dv1 == 10) dv1 = 0;
            if (dv2 == 10) dv2 = 0;
            if (dv1 == d[10] || dv2 == d[11]) return "";
            else return "Título eleitoral inválido";
        }
    }
}
