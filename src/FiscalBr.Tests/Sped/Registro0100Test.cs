using FiscalBr.Common;
using Newtonsoft.Json;
using System;
using System.Globalization;
using Xunit;

namespace FiscalBr.Tests.Sped
{
    public class Registro0100Test
    {
        protected  string expectedResult =
            $"|0100|NOME DO CONTADOR|11111111111|222222222222222|33333333333333|78700170|RUA DO ENDERECO|0|COMPLEMENTO|BAIRRO|11990001100|11880001100|EMAIL@EMAIL.COM.BR|5107602|{Environment.NewLine}";

        protected EFDContribuicoes.Bloco0.Registro0100 source = new EFDContribuicoes.Bloco0.Registro0100()
        {
            Nome = "NOME DO CONTADOR",
            Cpf = "11111111111",
            Crc = "222222222222222",
            Cnpj = "33333333333333",
            Cep = "78700170",
            End = "RUA DO ENDERECO",
            Num = "0",
            Compl = "COMPLEMENTO",
            Bairro = "BAIRRO",
            Fone= "11990001100",
            Fax= "11880001100",
            Email = "EMAIL@EMAIL.COM.BR",
            CodMun = "5107602"
        };

        [Fact]
        public void Escrever_Registro_0100_EFDFiscal()
        {
            var currentResult = Common.Sped.EscreverCamposSped.EscreverCampos(source, CodigoVersaoLeiaute.V17);

            Assert.Equal(expectedResult, currentResult);
        }

        [Fact]
        public void Ler_Registro_0100_EFDFiscal()
        {
            var currentResult = (EFDFiscal.Bloco0.Registro0100)Common.Sped.LerCamposSped.LerCampos(expectedResult);

            Assert.Equal(JsonConvert.SerializeObject(currentResult), JsonConvert.SerializeObject(source));
        }
    }
}
