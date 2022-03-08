using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiscalBr.Common;

namespace FiscalBr.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<CustomAtributosBenchmarks>();
            //NameParserBenchmarks nameParserBenchmarks = new NameParserBenchmarks();
            //nameParserBenchmarks.GetLastName();            
            Console.Read();
        }
    }

    [BenchmarkDotNet.Attributes.MemoryDiagnoser]
    public class CustomAtributosBenchmarks
    {
        List<System.Reflection.PropertyInfo> properties;
        public CustomAtributosBenchmarks()
        {
            properties = typeof(EFDFiscal.BlocoC.RegistroC500).GetProperties().OrderBy(p => p.GetCustomAttributes(typeof(Common.Sped.SpedCamposAttribute), true)
                .Cast<Common.Sped.SpedCamposAttribute>()
                .Select(a => a.Ordem)
                .FirstOrDefault())
                .ToList();
        }

        
        [BenchmarkDotNet.Attributes.Benchmark(Baseline = true)]
        public void Exist()
        {
            int versaoEspecifica = 16;
            foreach (var property in properties)
            {
                while (!ExisteAtributoPropriedadeParaVersao(property, versaoEspecifica))
                {
                    versaoEspecifica--;

                    if (versaoEspecifica < 1)
                        break;
                }
            }
                
            
        }

        [BenchmarkDotNet.Attributes.Benchmark()]
        public void ExistCache()
        {
            int versaoEspecifica = 16;
            foreach (var property in properties)
            {
                while (!ExisteAtributoPropriedadeParaVersaoCache(property, versaoEspecifica))
                {
                    versaoEspecifica--;

                    if (versaoEspecifica < 1)
                        break;
                }
            }


        }

        static Dictionary<string, Common.Sped.SpedCamposAttribute[]> SpedCamposAttributeRepository = new Dictionary<string, Common.Sped.SpedCamposAttribute[]>();

        private static bool ExisteAtributoPropriedadeParaVersaoCache(System.Reflection.PropertyInfo prop, int versao)
        {
            string propName = $"{ prop.DeclaringType.FullName}.{prop.Name}";
            if (!SpedCamposAttributeRepository.ContainsKey(propName))
                SpedCamposAttributeRepository.Add(propName, (Common.Sped.SpedCamposAttribute[])Attribute.GetCustomAttributes(prop, typeof(Common.Sped.SpedCamposAttribute), false));

            Common.Sped.SpedCamposAttribute[] attrs = SpedCamposAttributeRepository[propName];

            return attrs.Any(a => a.Versao == versao);
        }

        private bool ExisteAtributoPropriedadeParaVersao(System.Reflection.PropertyInfo prop, int versao)
        {
            var attrs = (Common.Sped.SpedCamposAttribute[])Attribute.GetCustomAttributes(prop, typeof(Common.Sped.SpedCamposAttribute));

            return attrs.Any(a => a.Versao == versao);
        }
    }
}
