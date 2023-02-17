using System;
using System.Collections.Generic;
using System.Linq;
using CTe.Classes;
using NFe.Classes;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Emitente;
using FiscalBr.EFDFiscal;
using static FiscalBr.EFDFiscal.Bloco0;
using static FiscalBr.EFDFiscal.BlocoC;
using static FiscalBr.EFDFiscal.BlocoD;
using static FiscalBr.EFDFiscal.BlocoE;
using static FiscalBr.EFDFiscal.BlocoH;

namespace FiscalBr.Parser
{
    public class SpedParser
    {
        public ArquivoEFDFiscal sped = new ArquivoEFDFiscal();

        private string cnpjEmitente = string.Empty;
        DateTime inicio;
        DateTime fim;
        public bool inicial;

        public SpedParser(ArquivoEFDFiscal _sped, DateTime? inicio = null, DateTime? fim = null)
        {
            this.inicio = inicio ?? DateTime.MinValue;
            this.fim = fim ?? DateTime.MinValue;
            sped = _sped ?? new ArquivoEFDFiscal();

            sped.Bloco0 = sped.Bloco0 ?? new Bloco0();
            sped.Bloco0.Reg0001 = sped.Bloco0.Reg0001 ?? new Registro0001();
            sped.Bloco0.Reg0001.Reg0100s = sped.Bloco0.Reg0001.Reg0100s ?? new List<Registro0100>();
            sped.Bloco0.Reg0001.Reg0150s = sped.Bloco0.Reg0001.Reg0150s ?? new List<Registro0150>();
            sped.Bloco0.Reg0001.Reg0190s = sped.Bloco0.Reg0001.Reg0190s ?? new List<Registro0190>();
            sped.Bloco0.Reg0001.Reg0200s = sped.Bloco0.Reg0001.Reg0200s ?? new List<Registro0200>();

            //ESCRITURAÇÃO E APURAÇÃO DO ISS
            sped.BlocoB = sped.BlocoB ?? new BlocoB();
            sped.BlocoB.RegB001 = sped.BlocoB.RegB001 ?? new BlocoB.RegistroB001() { IndDad = FiscalBr.Common.IndMovimento.BlocoSemDados };

            ///     BLOCO C: DOCUMENTOS FISCAIS I - MERCADORIAS (ICMS/IPI)
            sped.BlocoC = sped.BlocoC ?? new BlocoC();
            sped.BlocoC.RegC001 = sped.BlocoC.RegC001 ?? new RegistroC001();
            sped.BlocoC.RegC001.RegC100s = sped.BlocoC.RegC001.RegC100s ?? new List<RegistroC100>();

            ///     BLOCO D: DOCUMENTOS FISCAIS II - SERVIÇOS (ICMS)
            sped.BlocoD = sped.BlocoD ?? new BlocoD();
            sped.BlocoD.RegD001 = sped.BlocoD.RegD001 ?? new BlocoD.RegistroD001() { IndMov = FiscalBr.Common.IndMovimento.BlocoSemDados };
            sped.BlocoD.RegD001.RegD100s = sped.BlocoD.RegD001.RegD100s ?? new List<RegistroD100>();
            sped.BlocoC.RegC001.RegC100s = sped.BlocoC.RegC001.RegC100s ?? new List<RegistroC100>();

            ///     BLOCO E: APURAÇÃO DO ICMS E DO IPI
            sped.BlocoE = sped.BlocoE ?? new BlocoE();
            sped.BlocoE.RegE001 = sped.BlocoE.RegE001 ?? new RegistroE001();
            sped.BlocoE.RegE001.RegE100s = sped.BlocoE.RegE001.RegE100s ?? new List<RegistroE100>();
            if (!sped.BlocoE.RegE001.RegE100s.Any(w => w.DtIni == inicio))
                sped.BlocoE.RegE001.RegE100s.Add(AddPeridoApuracao(inicio ?? DateTime.MinValue, fim ?? DateTime.MinValue));

            ///     BLOCO G: CONTROLE DO CRÉDITO DE ICMS DO ATIVO PERMANENTE CIAP
            sped.BlocoG = sped.BlocoG ?? new BlocoG();
            sped.BlocoG.RegG001 = sped.BlocoG.RegG001 ?? new BlocoG.RegistroG001() { IndMov = FiscalBr.Common.IndMovimento.BlocoSemDados };

            ///     BLOCO H: INVENTÁRIO FÍSICO
            sped.BlocoH = sped.BlocoH ?? new BlocoH();
            sped.BlocoH.RegH001 = sped.BlocoH.RegH001 ?? new BlocoH.RegistroH001() { IndMov = FiscalBr.Common.IndMovimento.BlocoSemDados };

            ///     BLOCO K: CONTROLE DA PRODUÇÃO E DO ESTOQUE
            sped.BlocoK = sped.BlocoK ?? new BlocoK();
            sped.BlocoK.RegK001 = sped.BlocoK.RegK001 ?? new BlocoK.RegistroK001() { IndMov = FiscalBr.Common.IndMovimento.BlocoSemDados };

            ///     BLOCO 1: OUTRAS INFORMAÇÕES
            sped.Bloco1 = sped.Bloco1 ?? new Bloco1();
            sped.Bloco1.Reg1001 = sped.Bloco1.Reg1001 ?? new Bloco1.Registro1001() { IndMov = FiscalBr.Common.IndMovimento.BlocoComDados, Reg1010 = new Bloco1.Registro1010() };

        }

        public void Parser(DateTime inicio, DateTime fim)
        {
            this.inicio = inicio;
            this.fim = fim;
        }

        private RegistroE100 AddPeridoApuracao(DateTime inicio, DateTime fim)
        {
            return new RegistroE100() { DtIni = inicio, DtFin = fim, RegE110 = new RegistroE110() };
        }

        public void Gravar(string path)
        {
            TotalizaImpostos();
            sped.GerarLinhas();
            sped.CalcularBloco9();
            sped.Escrever($"{path}\\TagOne-SpedEFD-{sped?.Bloco0?.Reg0000?.Cnpj}-{sped?.Bloco0?.Reg0000?.DtIni:yyMMdd}-{sped?.Bloco0?.Reg0000?.DtFin:yyMMdd}");
        }

        public string Finalizar()
        {
            TotalizaImpostos();
            sped.GerarLinhas();
            sped.CalcularBloco9();
            return string.Join(Environment.NewLine, sped.Linhas.ToArray()) + Environment.NewLine;
        }

        private void TotalizaImpostos()
        {
            var total = sped.BlocoE.RegE001.RegE100s[0].RegE110;

            //soma dos credito das notas de entrada
            total.VlTotCreditos = sped.BlocoC.RegC001.RegC100s.Where(w => w.RegC190s != null).SelectMany(w => w.RegC190s.Where(c => c.Cfop < 4000)).Sum(s => s.VlIcms);

            //soma credito notas de transporte.
            total.VlTotCreditos += sped.BlocoD.RegD001.RegD100s.Where(w => w.RegD190s != null).SelectMany(w => w.RegD190s).Sum(w => w.VlIcms);

            //soma dos debitos notas de saida
            total.VlTotDebitos = sped.BlocoC.RegC001.RegC100s.Where(w => w.RegC190s != null).SelectMany(w => w.RegC190s.Where(c => c.Cfop > 4000 || c.Cfop == 1605)).Sum(s => s.VlIcms);

            //soma dos ajustes a debitos notas de saida
            total.VlAjDebitos = sped.BlocoC.RegC001.RegC100s.Where(w => w.RegC195s != null).SelectMany(w => w.RegC195s)
                .Where(w => w.RegC197s != null).SelectMany(w => w.RegC197s).Sum(s => s.VlIcms);

            decimal totalDebitos = total.VlTotDebitos + total.VlAjDebitos;

            if (totalDebitos > total.VlTotCreditos)
                total.VlSldApurado = total.VlIcmsRecolher = totalDebitos - total.VlTotCreditos;
            else
                total.VlSldCredorTransportar = total.VlTotCreditos - totalDebitos;

            total.RegE116s = new List<RegistroE116>();
            total.RegE116s.Add(new RegistroE116() { CodOr = "000", VlOr = total.VlIcmsRecolher, DtVcto = new DateTime(fim.Year, fim.Month, 20), CodRec = "1112" });

            if (sped.BlocoD.RegD001.RegD100s.Count > 0 || sped.BlocoD.RegD001.RegD500s?.Count > 0)
                sped.BlocoD.RegD001.IndMov = FiscalBr.Common.IndMovimento.BlocoComDados;
        }

        //public void AddContador(Pessoa pessoa)
        //{
        //    sped.Bloco0.Reg0001.Reg0100s = sped.Bloco0.Reg0001.Reg0100s ?? new List<Registro0100>();
        //    sped.Bloco0.Reg0001.Reg0100s.Add(ParseContador(pessoa));
        //}

        //public void AddEmpresa(Pessoa pessoa)
        //{
        //    sped.Bloco0.Reg0000 = ParseEmpresa(pessoa);
        //    sped.Bloco0.Reg0001.Reg0005 = ParseEmpresaComplemento(pessoa);
        //}

        //private Registro0005 ParseEmpresaComplemento(Pessoa pessoa)
        //{
        //    PessoaEndereco endereco = pessoa.PessoaEnderecos.FirstOrDefault(w => w.Situacao && w.Padrao);
        //    return new Registro0005()
        //    {
        //        Fantasia = pessoa.NomeFantasia,
        //        Cep = endereco.CEP.LimpaNumero(),
        //        End = endereco.Logradouro,
        //        Num = endereco.Numero,
        //        Bairro = endereco.Bairro?.NomeBairro
        //    };
        //}

        //        private Registro0000 ParseEmpresa(Pessoa pessoa)
        //        {
        //            PessoaEndereco endereco = pessoa.PessoaEnderecos.FirstOrDefault(w => w.Situacao && w.Padrao);
        //            return sped.Bloco0.Reg0000 = new Registro0000()
        //            {
        //                CodVer = FiscalBr.Common.CodigoVersaoLeiaute.V17,
        //                Cnpj = pessoa.CpfCnpj.LimpaNumero(),
        //                Ie = pessoa.InscricaoEstadual.LimpaNumero(),
        //                Nome = pessoa.NomePessoa,
        //                Uf = endereco.CodigoEstado,
        //                CodMun = endereco.Cidade.CodigoIBGE.ToString(),
        //#warning verificar uma configuracao para esse perfil
        //                IndPerfil = FiscalBr.Common.IndPerfilArquivo.A,
        //                IndAtiv = FiscalBr.Common.IndTipoAtividade.Outros,
        //                DtIni = inicio,
        //                DtFin = fim
        //            };
        //        }

        //private Registro0100 ParseContador(Pessoa pessoa)
        //{
        //    PessoaEndereco endereco = pessoa.PessoaEnderecos.FirstOrDefault(w => w.Situacao && w.Padrao);
        //    return new Registro0100()
        //    {
        //        Nome = pessoa.NomePessoa,
        //        Cpf = pessoa.CpfCnpj.LimpaNumero(),
        //        Cnpj = pessoa.InscricaoEstadual.LimpaNumero(),
        //        Crc = pessoa.InscricaoMunicipal.LimpaNumero(),
        //        Email = pessoa.Email,
        //        Cep = endereco.CEP.LimpaNumero(),
        //        End = endereco.Logradouro,
        //        Num = endereco.Numero,
        //        Compl = endereco.Complemento,
        //        Bairro = endereco.NomeBairro,
        //        CodMun = endereco.Cidade.CodigoIBGE.ToString()
        //    };
        //}

        public void AddNota(nfeProc nfe, int situacaoNota, bool propria, int es, DateTime? dataMovimentacao)
        {
            //adiciona participant
            if (situacaoNota == 0 && nfe.NFe.infNFe.ide.mod == DFe.Classes.Flags.ModeloDocumento.NFe)
                AddParticipante(nfe, propria);



            RegistroC100 nota = ParseNota(nfe, situacaoNota, propria, es, dataMovimentacao);

            if (!propria)
            {
                AddProdutos(nfe);

                nota.RegC170s = new List<RegistroC170>();
                //adiciona produtos da nota
                foreach (var prod in nfe.NFe.infNFe.det)
                {
                    var produto = ParseProdutoNota(prod, propria, nfe.NFe.infNFe.ide.indFinal == NFe.Classes.Informacoes.Identificacao.Tipos.ConsumidorFinal.cfConsumidorFinal);
                    //AJUSTA ICMS ST E IPI PARA NOTAS DE ENTRADA
                    if (!propria && produto.VlIcmsSt > 0)
                    {
                        nota.VlOutDa += produto.VlIcmsSt;
                        produto.VlIcmsSt = produto.VlBcIcmsSt = produto.AliqSt = 0;

                    }
                    nota.RegC170s.Add(produto);
                }
            }
            sped.BlocoC.RegC001.RegC100s.Add(nota);

        }

        public void AddNota(cteProc cte, int? modalidade, DateTime? dataMovimentacao)
        {

            //adiciona participant
            AddParticipant(cte);

            RegistroD100 nota = ParseNota(cte, modalidade ?? 0, dataMovimentacao);
            sped.BlocoD.RegD001.RegD100s.Add(nota);
        }

        //public void AddNota(Nota _nota)
        //{
        //    var servico = _nota.NotaProdutoXml.FirstOrDefault();

        //    if (servico == null || servico.CodigoCFOP < 1000)
        //        return;

        //    AddParticipant(_nota);
        //    string cfopGrupo = servico.CodigoCFOP.ToString().Substring(0, 2);
        //    if (cfopGrupo == "53" || cfopGrupo == "63") // CFOP DE TELECOMUNICACAO
        //    {
        //        RegistroD500 registro = ParseNota(_nota);
        //        if (sped.BlocoD.RegD001.RegD500s == null)
        //            sped.BlocoD.RegD001.RegD500s = new List<RegistroD500>();
        //        sped.BlocoD.RegD001.RegD500s.Add(registro);
        //    }
        //    else if (cfopGrupo == "52" || cfopGrupo == "62") //CFOP DE CONTA DE LUZ
        //    {
        //        RegistroC500 registro = ParseNotaC500(_nota);
        //        if (sped.BlocoC.RegC001.RegC500s == null)
        //            sped.BlocoC.RegC001.RegC500s = new List<RegistroC500>();
        //        sped.BlocoC.RegC001.RegC500s.Add(registro);
        //    }
        //}

        private void AddProdutos(nfeProc nfe)
        {
            foreach (var prod in nfe.NFe.infNFe.det)
            {
                AddUnidadeMedida(prod);

                AddProduto(prod);
            }
        }

        private void AddParticipante(nfeProc nfe, bool propria)
        {
            if (nfe.NFe.infNFe.ide.mod == DFe.Classes.Flags.ModeloDocumento.NFe)
            {
                if (!propria)
                {
#warning REMOVER CARACTERES DO CNPJ
                    if (!sped.Bloco0.Reg0001.Reg0150s.Any(w => w.CodPart == nfe.NFe.infNFe.emit.CNPJ))
                        sped.Bloco0.Reg0001.Reg0150s.Add(ParseParticipante(nfe.NFe.infNFe.emit));
                }
                else if (!string.IsNullOrEmpty(nfe.NFe.infNFe.dest.CNPJ))
                {
#warning LIMPAR CNPJ
                    if (!sped.Bloco0.Reg0001.Reg0150s.Any(w => w.CodPart == nfe.NFe.infNFe.dest.CNPJ))
                        sped.Bloco0.Reg0001.Reg0150s.Add(ParseParticipante(nfe.NFe.infNFe.dest));
                }
                else
#warning LIMPAR CNPJ
                    if (!sped.Bloco0.Reg0001.Reg0150s.Any(w => w.CodPart == nfe.NFe.infNFe.dest.CPF))
                    sped.Bloco0.Reg0001.Reg0150s.Add(ParseParticipante(nfe.NFe.infNFe.dest));
            }
        }

        private void AddParticipant(cteProc cte)
        {
#warning LIMPAR CNPJ
            if (!sped.Bloco0.Reg0001.Reg0150s.Any(w => w.CodPart == cte.CTe.infCte.emit.CNPJ))
                sped.Bloco0.Reg0001.Reg0150s.Add(ParseParticipante(cte.CTe.infCte.emit));
        }

        //private void AddParticipant(Nota nota)
        //{
        //    if (!sped.Bloco0.Reg0001.Reg0150s.Any(w => w.CodPart == nota.Emitente.CpfCnpj.LimpaNumero()))
        //        sped.Bloco0.Reg0001.Reg0150s.Add(ParseParticipante(nota.Emitente));
        //}

        //private Registro0150 ParseParticipante(Pessoa emit)
        //{
        //    try
        //    {
        //        var endereco = emit.PessoaEnderecos.FirstOrDefault(w => w.Padrao);
        //        return new Registro0150()
        //        {
        //            CodPart = emit.CpfCnpj.LimpaNumero(),
        //            Nome = emit.NomePessoa,
        //            Cnpj = emit.CpfCnpj.LimpaNumero(),
        //            Ie = emit.InscricaoEstadual,
        //            CodPais = "1058",
        //            CodMun = endereco.Cidade.CodigoIBGE.ToString(),
        //            End = endereco.Logradouro,
        //            Num = endereco.Numero,
        //            Compl = endereco.Complemento,
        //            Bairro = endereco.Bairro.NomeBairro
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("VERIFIQUE O CADASTRO DO ENDEREÇO DO EMITENTE", ex);
        //    }
        //}

        private Registro0150 ParseParticipante(CTe.Classes.Informacoes.Emitente.emit emit)
        {
            return new Registro0150()
            {
#warning LIMPAR CNPJ
                CodPart = emit.CNPJ,
                Nome = emit.xNome,
                Cnpj = emit.CNPJ,
                Ie = emit.IE,
                CodPais = "1058",
                CodMun = emit.enderEmit.cMun.ToString(),
                End = emit.enderEmit.xLgr,
                Num = emit.enderEmit.nro,
                Compl = emit.enderEmit.xCpl,
                Bairro = emit.enderEmit.xBairro
            };
        }

        private void AddProduto(det produto)
        {
            if (!sped.Bloco0.Reg0001.Reg0200s.Any(w => w.CodItem == produto.prod.cProd))
                sped.Bloco0.Reg0001.Reg0200s.Add(ParseProduto(produto));
        }

        //private void AddProduto(InventarioEstoque produto, decimal _percentualIcms)
        //{
        //    if (!sped.Bloco0.Reg0001.Reg0200s.Any(w => w.CodItem == produto.CodigoProdutoEstoque.ToString()))
        //        sped.Bloco0.Reg0001.Reg0200s.Add(ParseProduto(produto, _percentualIcms));
        //}

        RegistroC170 ParseProdutoNota(det produto, bool propria, bool insumo = false)
        {

            int cst = ParseCST(produto.imposto.ICMS.TipoICMS);
            int cfop = insumo ? 1556 : produto.prod.CFOP >= 5150 && produto.prod.CFOP <= 5156 ? 1152 : 2102;

            decimal pICMS = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "pICMS");
            decimal vBC = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "vBC");
            decimal vICMS = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "vICMS");

            //SE NOTA DE ENTRADA EMITIDA POR SIMPLES NACIONAL NAO TEM CREDITO DE ICMS
            if (!propria)
            {
                int outcst = this.ParseCst(cst, insumo);
                if (outcst != cst)
                {
                    cst = outcst;

                    pICMS = vBC = vICMS = 0;
                }
            }

            decimal pICMSST = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "pICMSST");
            decimal vBCST = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "vBCST");
            decimal vICMSST = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "vICMSST");


            //PIS
            int cstPIS = 70;
            decimal AliqPis = 0, VlBcPis = 0, VlPis = 0;
            if (propria)
            {
                cstPIS = ParseCST(produto.imposto.PIS.TipoPIS);
                AliqPis = GetValor<decimal>(produto.imposto.PIS.TipoPIS, "pPIS");
                VlBcPis = GetValor<decimal>(produto.imposto.PIS.TipoPIS, "vBC");
                VlPis = GetValor<decimal>(produto.imposto.PIS.TipoPIS, "vPIS");
            }

            //COFINS
            int cstCOFINS = 70;
            decimal AliqCofins = 0, VlBcCofins = 0, VlCofins = 0;
            if (propria)
            {
                cstCOFINS = ParseCST(produto.imposto.COFINS.TipoCOFINS);
                AliqCofins = GetValor<decimal>(produto.imposto.COFINS.TipoCOFINS, "pCOFINS");
                VlBcCofins = GetValor<decimal>(produto.imposto.COFINS.TipoCOFINS, "vBC");
                VlCofins = GetValor<decimal>(produto.imposto.COFINS.TipoCOFINS, "vCOFINS");
            }

            //IPI
            string cstIPI = "49";
            decimal AliqIPI = 0, VlBcIPI = 0, VlIPI = 0;
            if (propria)
            {
                cstIPI = GetValor<string>(produto.imposto?.IPI?.TipoIPI, "CST");
                AliqIPI = GetValor<decimal>(produto.imposto?.IPI?.TipoIPI, "pIPI");
                VlBcIPI = GetValor<decimal>(produto.imposto?.IPI?.TipoIPI, "vBC");
                VlIPI = GetValor<decimal>(produto.imposto?.IPI?.TipoIPI, "vIPI");
            }
            else
                produto.prod.vProd += GetValor<decimal>(produto.imposto?.IPI?.TipoIPI, "vIPI");



            return new RegistroC170()
            {
                NumItem = produto.nItem,
                CodItem = produto.prod.cProd,
                CstIcms = cst,
                Cfop = cfop,
                Unid = produto.prod.uCom,
                IndMov = FiscalBr.Common.IndMovFisicaItem.Nao,
                Qtd = produto.prod.qCom,
                VlItem = produto.prod.vProd,
                VlDesc = produto.prod.vDesc ?? 0,
                //ICMS
                AliqIcms = pICMS,
                VlBcIcms = vBC,
                VlIcms = vICMS,
                //ICMS ST
                AliqSt = pICMSST,
                VlBcIcmsSt = vBCST,
                VlIcmsSt = vICMSST,
                //PIS
                CstPis = cstPIS,
                AliqPis = AliqPis,
                VlBcPis = VlBcPis,
                VlPis = VlPis,
                //COFINS
                CstCofins = cstCOFINS,
                AliqCofins = AliqCofins,
                VlBcCofins = VlBcCofins,
                VlCofins = VlCofins,
                //
                CstIpi = cstIPI,
                AliqIpi = AliqIPI,
                VlBcIpi = VlBcIPI,
                VlIpi = VlIPI,

            };
        }

        Registro0150 ParseParticipante(emit emit)
        {
            return new Registro0150()
            {
#warning LIMPAR CNPJ
                CodPart = emit.CNPJ,
                Nome = emit.xNome,
                Cnpj = emit.CNPJ,
                Ie = emit.IE,
                CodPais = "1058",
                CodMun = emit.enderEmit.cMun.ToString(),
                End = emit.enderEmit.xLgr,
                Num = emit.enderEmit.nro,
                Compl = emit.enderEmit.xCpl,
                Bairro = emit.enderEmit.xBairro
            };

        }

        Registro0150 ParseParticipante(dest dest)
        {
            string doc = dest.CNPJ != null ? dest.CNPJ : dest.CPF;
            return new Registro0150()
            {
#warning LIMPAR CNPJ
                CodPart = doc,
                Nome = dest.xNome,
                Cpf = dest.CPF,
                Cnpj = dest.CNPJ,
                Ie = dest.IE,
                CodPais = "1058",
                CodMun = dest.enderDest.cMun.ToString(),
                End = dest.enderDest.xLgr,
                Num = dest.enderDest.nro,
                Compl = dest.enderDest.xCpl,
                Bairro = dest.enderDest.xBairro
            };

        }

        void AddUnidadeMedida(det produto)
        {
            if (!sped.Bloco0.Reg0001.Reg0190s.Any(w => w.Unid?.ToUpper() == produto.prod?.uCom?.ToUpper()))
                sped.Bloco0.Reg0001.Reg0190s.Add(
                    new Registro0190() { Descr = produto.prod.uCom, Unid = produto.prod.uCom });
        }

        //void AddUnidadeMedida(InventarioEstoque produto)
        //{
        //    if (!sped.Bloco0.Reg0001.Reg0190s.Any(w => w.Unid == produto.Unidade))
        //        sped.Bloco0.Reg0001.Reg0190s.Add(
        //            new Registro0190() { Descr = produto.Unidade, Unid = produto.Unidade });
        //}

        Registro0200 ParseProduto(det produto)
        {
            decimal percentualIcms = 0;
            try
            {
                percentualIcms = GetValor<decimal>(produto.imposto.ICMS.TipoICMS, "pICMS");
            }
            catch (Exception) { }

            var reg = new Registro0200()
            {
                CodItem = produto.prod.cProd,
                DescrItem = produto.prod.xProd,
                UnidInv = produto.prod.uCom,
                TipoItem = "04",
                CodNcm = produto.prod.NCM,
                CodGen = produto.prod.NCM,
                AliqIcms = percentualIcms,
            };

            if (this.inicial && !string.IsNullOrEmpty(produto.prod.cBenef))
            {
                reg.Reg0205s = new List<Registro0205>();
                reg.Reg0205s.Add(new Registro0205()
                {
                    DescrAntItem = produto.prod.xProd,
                    DtIni = new DateTime(2018, 1, 1),
                    DtFin = new DateTime(2021, 12, 31),
                    CodAntItem = produto.prod.cBenef
                });
            }
            return reg;
        }

        //Registro0200 ParseProduto(InventarioEstoque produto, decimal _percentualIcms)
        //{
        //    decimal percentualIcms = _percentualIcms;

        //    var reg = new Registro0200()
        //    {
        //        CodItem = produto.CodigoProdutoEstoque.ToString(),
        //        DescrItem = produto.NomeProduto,
        //        UnidInv = produto.Unidade,
        //        TipoItem = "04",
        //        CodNcm = produto.Ncm,
        //        CodGen = produto.Ncm,
        //        AliqIcms = percentualIcms
        //    };

        //    if (this.inicial && !string.IsNullOrEmpty(produto.CodigoMigracao))
        //    {
        //        reg.Reg0205s = new List<Registro0205>();
        //        reg.Reg0205s.Add(new Registro0205()
        //        {
        //            DescrAntItem = produto.NomeProduto,
        //            DtIni = new DateTime(2018, 1, 1),
        //            DtFin = new DateTime(2021, 12, 31),
        //            CodAntItem = produto.CodigoMigracao
        //        });
        //    }
        //    return reg;
        //}

        RegistroC100 ParseNota(nfeProc nfe, int situacaoNota, bool propria, int es, DateTime? dataMovimentacao)
        {
            string CodPart = null;
            if (nfe.NFe.infNFe.ide.mod == DFe.Classes.Flags.ModeloDocumento.NFe)
            {
                if (!propria)
#warning LIMPAR CNPJ
                    CodPart = nfe.NFe.infNFe.emit.CNPJ;
                else if (nfe.NFe.infNFe.dest.CNPJ != null)
                    CodPart = nfe.NFe.infNFe.dest.CNPJ;
                else
                    CodPart = nfe.NFe.infNFe.dest.CPF;
            }

            if (situacaoNota == 5 || situacaoNota == 2 || situacaoNota == 4)
                return new RegistroC100()
                {
                    CodMod = ((int)nfe.NFe.infNFe.ide.mod).ToString(),
                    IndOper = es,
                    CodSit = situacaoNota,
                    NumDoc = nfe.NFe.infNFe.ide.nNF.ToString(),
                    Ser = nfe.NFe.infNFe.ide.serie.ToString(),
                    ChvNfe = nfe.protNFe?.infProt?.chNFe
                };

            var RegC190s = ParseNotaAnalitico(nfe, situacaoNota, propria);

            return new RegistroC100()
            {
                CodMod = ((int)nfe.NFe.infNFe.ide.mod).ToString(),
                IndOper = es,
                CodSit = situacaoNota,
                IndEmit = propria ? 0 : 1,
                CodPart = CodPart,
                NumDoc = nfe.NFe.infNFe.ide.nNF.ToString(),
                Ser = nfe.NFe.infNFe.ide.serie.ToString(),
                DtDoc = nfe.NFe.infNFe.ide.dhEmi.DateTime,
                DtEs = propria ? nfe.NFe.infNFe.ide.dhEmi.DateTime : dataMovimentacao ?? nfe.NFe.infNFe.ide.dhEmi.DateTime,
                IndPgto = parseTipoPagamento(nfe),
                IndFrt = parseTipoFrete(nfe),
                ChvNfe = nfe.protNFe?.infProt?.chNFe,
                VlBcIcms = RegC190s.Sum(v => v.VlBcIcms),
                VlIcms = RegC190s.Sum(v => v.VlIcms),
                VlDoc = nfe.NFe.infNFe.total.ICMSTot.vNF,
                VlMerc = nfe.NFe.infNFe.total.ICMSTot.vProd + (!propria && es == 0 ? nfe.NFe.infNFe.total.ICMSTot.vIPI : 0),
                VlDesc = nfe.NFe.infNFe.total.ICMSTot.vDesc,
                VlFrt = nfe.NFe.infNFe.total.ICMSTot.vFrete,
                VlOutDa = nfe.NFe.infNFe.total.ICMSTot.vOutro,
                RegC190s = RegC190s,
            };
        }

        private RegistroD100 ParseNota(cteProc cte, int modalidade, DateTime? dataMovimentacao)
        {

            return new RegistroD100()
            {
                CodMod = ((int)cte.CTe.infCte.ide.mod).ToString(),
                IndOper = 0,
                CodSit = 0,
                IndEmit = 1,
#warning LIMPAR CNPJ
                CodPart = cte.CTe.infCte.emit.CNPJ,
                NumDoc = cte.CTe.infCte.ide.nCT.ToString(),
                ChvCte = cte.protCTe.infProt.chCTe,
                Ser = cte.CTe.infCte.ide.serie.ToString(),
                DtDoc = cte.CTe.infCte.ide.dhEmi.DateTime,
                DtAP = dataMovimentacao ?? cte.CTe.infCte.ide.dhEmi.DateTime,
                //DtEs = nfe.NFe.infNFe.ide.dhSaiEnt.HasValue ? (DateTime?)nfe.NFe.infNFe.ide.dhSaiEnt.Value.DateTime : nfe.NFe.infNFe.ide.dhEmi.DateTime,
                //IndPgto = parseTipoPagamento(nfe),
                TpCte = (int)cte.CTe.infCte.ide.tpCTe,
                IndFrt = modalidade,
                VlBcIcms = GetValor<decimal>(cte.CTe.infCte.imp.ICMS.TipoICMS, "vBC"),
                VlIcms = GetValor<decimal>(cte.CTe.infCte.imp.ICMS.TipoICMS, "vICMS"),
                VlDoc = cte.CTe.infCte.vPrest.vTPrest,
                VlServ = cte.CTe.infCte.vPrest.vRec,
                CodMunOrig = cte.CTe.infCte.ide.cMunEnv.ToString(),
                CodMunDest = cte.CTe.infCte.ide.cMunFim.ToString(),
                RegD190s = new List<RegistroD190>() {
                    new RegistroD190()
                    {
                        CstIcms = 0,
                        Cfop = cte.CTe.infCte.ide.CFOP - 4000,
                        AliqIcms = GetValor<decimal>(cte.CTe.infCte.imp.ICMS.TipoICMS, "pICMS"),
                        VlOpr = GetValor<decimal>(cte.CTe.infCte.imp.ICMS.TipoICMS, "vBC"),
                        VlBcIcms = GetValor<decimal>(cte.CTe.infCte.imp.ICMS.TipoICMS, "vBC"),
                        VlIcms = GetValor<decimal>(cte.CTe.infCte.imp.ICMS.TipoICMS, "vICMS")
                    }
                }
            };
        }

        //private RegistroD500 ParseNota(Nota nota)
        //{
        //    return new RegistroD500()
        //    {
        //        CodMod = "22",
        //        IndOper = "0",
        //        CodSit = "00",
        //        IndEmit = "1",
        //        CodPart = nota.Emitente.CpfCnpj.LimpaNumero(),
        //        NumDoc = (decimal)nota.NumeroNota,
        //        Ser = nota.SerieNota,
        //        DtDoc = nota.DataEmissao,
        //        DtAP = nota.DataMovimentacao,
        //        VlDoc = nota.ValorNota,
        //        VlServ = nota.ValorNota,
        //        RegD590s = new List<RegistroD590>() {
        //            new RegistroD590()
        //            {
        //                CstIcms = 0,
        //                Cfop = nota.NotaProdutoXml.FirstOrDefault().CodigoCFOP - 4000,
        //                VlOpr = nota.ValorNota
        //            }
        //        }
        //    };
        //}

        //private RegistroC500 ParseNotaC500(Nota nota)
        //{
        //    return new RegistroC500()
        //    {

        //        CodMod = "66",
        //        ChvDoce = nota.ChaveNota,
        //        IndOper = FiscalBr.Common.IndTipoOperacaoProduto.Entrada,
        //        CodSit = FiscalBr.Common.IndCodSitDoc.DocumentoRegular,
        //        IndEmit = FiscalBr.Common.IndEmitente.Terceiros,
        //        CodPart = nota.Emitente.CpfCnpj.LimpaNumero(),
        //        NumDoc = (long)nota.NumeroNota,
        //        Ser = nota.SerieNota,
        //        DtDoc = nota.DataEmissao,
        //        DtEs = nota.DataMovimentacao ?? nota.DataEmissao,
        //        VlDoc = nota.ValorNota,
        //        RegC590s = new List<RegistroC590>() {
        //            new RegistroC590()
        //            {
        //                CstIcms = 0,
        //                Cfop = nota.NotaProdutoXml.FirstOrDefault().CodigoCFOP - 4000,
        //                VlOpr = nota.ValorNota
        //            }
        //        }
        //    };
        //}

        List<RegistroC190> ParseNotaAnalitico(nfeProc nfe, int situacaoNota, bool propria)
        {
            bool insumo = !propria && nfe.NFe.infNFe.ide.indFinal == NFe.Classes.Informacoes.Identificacao.Tipos.ConsumidorFinal.cfConsumidorFinal;
            if (situacaoNota == 2)
                return null;
            List<RegistroC190> ret = new List<RegistroC190>();
            foreach (var item in nfe.NFe.infNFe.det)
            {
                int calcCFOP = insumo ? 1556 : propria ? item.prod.CFOP : (item.prod.CFOP >= 5150 && item.prod.CFOP <= 5156 ? 1152 : 2102);
                int cst = ParseCST(item.imposto.ICMS.TipoICMS);

                decimal pICMS = GetValor<decimal>(item.imposto.ICMS.TipoICMS, "pICMS");
                decimal vBC = GetValor<decimal>(item.imposto.ICMS.TipoICMS, "vBC");
                decimal vICMS = GetValor<decimal>(item.imposto.ICMS.TipoICMS, "vICMS");

                //SE NOTA DE ENTRADA EMITIDA POR SIMPLES NACIONAL NAO TEM CREDITO DE ICMS
                if (!propria)
                {
                    int outcst = this.ParseCst(cst, insumo);
                    if (outcst != cst)
                    {
                        cst = outcst;

                        pICMS = vBC = vICMS = 0;
                    }
                }

                decimal vlOpr = item.prod.vProd - (item.prod.vDesc ?? 0);
                decimal vlRedBc = 0;


                if (vBC > 0 && vlOpr > vBC)
                    vlRedBc = vlOpr - vBC;

                decimal vBCST = GetValor<decimal>(item.imposto.ICMS.TipoICMS, "vBCST");
                decimal vST = GetValor<decimal>(item.imposto.ICMS.TipoICMS, "vBCST");
                decimal vBCSTRet = GetValor<decimal>(item.imposto.ICMS.TipoICMS, "vBCSTRet");
                if (vBCSTRet > 0 && vlOpr > vBCSTRet)
                    vlRedBc = vlOpr - vBCSTRet;

                decimal vIPI = GetValor<decimal>(item.imposto.IPI, "vIPI");

                if (insumo)
                    pICMS = vICMS = vBC = vBCST = vST = vlRedBc = 0;

                //AJUSTA ICMS ST PARA NOTAS DE ENTRADA
                if (!propria && vST > 0)
                    vBCST = vST = 0;

                RegistroC190 reg = ret.FirstOrDefault(w => w.CstIcms == cst && w.Cfop == calcCFOP && w.AliqIcms == pICMS);

                if (reg != null)
                {
                    reg.VlOpr += item.prod.vProd - (item.prod.vDesc ?? 0);
                    reg.VlBcIcms += vBC;
                    reg.VlIcms += vICMS;
                    reg.VlBcIcmsSt += vBCST;
                    reg.VlIcmsSt += vST;
                    reg.VlRedBc += vlRedBc;

                }
                else
                    ret.Add(new RegistroC190()
                    {
                        CstIcms = cst,
                        Cfop = calcCFOP,
                        AliqIcms = pICMS,
                        VlOpr = item.prod.vProd - (item.prod.vDesc ?? 0),
                        VlBcIcms = vBC,
                        VlIcms = vICMS,
                        VlBcIcmsSt = vBCST,
                        VlIcmsSt = vST,
                        VlRedBc = vlRedBc
                    });

            }
            return ret;
        }

        private int parseTipoFrete(nfeProc nfe)
        {
            if (nfe.NFe.infNFe.transp.modFrete == NFe.Classes.Informacoes.Transporte.ModalidadeFrete.mfProprioContaRemente)
                return 1;
            else if (nfe.NFe.infNFe.transp.modFrete == NFe.Classes.Informacoes.Transporte.ModalidadeFrete.mfContaDestinatario ||
                nfe.NFe.infNFe.transp.modFrete == NFe.Classes.Informacoes.Transporte.ModalidadeFrete.mfProprioContaDestinatario)
                return 2;
            else if (nfe.NFe.infNFe.transp.modFrete == NFe.Classes.Informacoes.Transporte.ModalidadeFrete.mfContaTerceiros)
                return 3;

            return 9;
        }

        int parseTipoPagamento(nfeProc nfe)
        {
            if (nfe.NFe.infNFe.pag.Any(w => w.tPag == NFe.Classes.Informacoes.Pagamento.FormaPagamento.fpOutro))
                return 2;
            else if (nfe.NFe.infNFe.pag.Any(w => w.tPag == NFe.Classes.Informacoes.Pagamento.FormaPagamento.fpDinheiro))
                return 0;
            else
                return 1;

        }


        public int ParseCST(object obj)
        {
#warning LIMPAR CNPJ
            var pObj = obj.GetType().GetProperty("CST");
            if (pObj != null)
                return (int)Convert.ChangeType(pObj.GetValue(obj).ToString(), typeof(int));

            pObj = obj.GetType().GetProperty("CSOSN");
            if (pObj != null)
                return (int)Convert.ChangeType(pObj.GetValue(obj).ToString(), typeof(int));

            return -1;
        }

        public T GetValor<T>(object obj, string nomeColuna, bool multiplo = false)
        {
            if (obj == null)
                return (T)Convert.ChangeType("0", typeof(T));

            try
            {
                var pObj = obj.GetType().GetProperty(nomeColuna);

                if (pObj == null && !multiplo)
                    return (T)Convert.ChangeType("0", typeof(T));
                else if (pObj == null && multiplo)
                    throw new Exception("Coluna invalida!");

                object value = pObj.GetValue(obj);
                if (value == null)
                    return (T)Convert.ChangeType("0", typeof(T));
                return (T)Convert.ChangeType(value.ToString(), typeof(T));
            }
            catch (Exception)
            {
                return (T)Convert.ChangeType("0", typeof(T));
            }

        }

        //internal void ParseInventario(IEnumerable<InventarioEstoque> produtos, TributacaoRegra tributacao)
        //{
        //    if (produtos.Count() > 0)
        //    {
        //        sped.BlocoH.RegH001.IndMov = FiscalBr.Common.IndMovimento.BlocoComDados;
        //        RegistroH005 H005 = new RegistroH005() { DtInv = new DateTime(inicio.Year - 1, 12, 31), MotInv = 1, RegH010s = new List<RegistroH010>() };
        //        sped.BlocoH.RegH001.RegH005s = new List<RegistroH005>() { H005 };

        //        foreach (var prod in produtos)
        //            H005.RegH010s.Add(ParseInventarioProduto(prod, tributacao));

        //        H005.VlInv = H005.RegH010s.Sum(w => w.VlItem);
        //    }
        //}

        //private RegistroH010 ParseInventarioProduto(InventarioEstoque produto, TributacaoRegra tributacao)
        //{
        //    AddUnidadeMedida(produto);
        //    AddProduto(produto, tributacao.TributacaoICMS.AlicotaICMS);
        //    return new RegistroH010()
        //    {
        //        CodItem = produto.CodigoProdutoEstoque.ToString(),
        //        Unid = produto.Unidade,
        //        Qtd = produto.QuantidadeAtual,
        //        VlUnit = produto.ValorCusto,
        //        VlItem = produto.ValorCusto * produto.QuantidadeAtual,
        //        VlItemIr = produto.ValorCusto * produto.QuantidadeAtual,
        //        CodCta = "1.13.010.01",
        //        RegH020s = new List<RegistroH020>() { new RegistroH020() { CstIcms = "0", BcIcms = produto.ValorCusto, VlIcms = produto.ValorCusto * (tributacao.TributacaoICMS.AlicotaICMS / 100) } }
        //    };
        //}

        private int ParseCst(int cst, bool insumo)
        {

            int[] csts = { 40, 41, 400, 101, 102 };
            if ((csts.Contains(cst) || insumo))
                cst = 90;
            else if (cst == 10)
                cst = 0;

            return cst;
        }

    }
}
