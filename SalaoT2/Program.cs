using SalaoT2.Dominio;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using static SalaoT2.Dominio.Agendamento;

namespace SalaoT2
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var meusClientes = IncluirMeusClientes();
                var meusServicos = IncluirMeusServicos();
                var meusFuncionarios = IncluirFuncionarios(meusServicos);

                meusFuncionarios.ExcluirServicoDeUmFuncionario(10, 1);

                //meusClientes.AlterarUmCliente(1, "Diego", "199999999");
                //meusClientes.ExcluirUmCliente(2);

                List<Agendamento> agenda = new List<Agendamento>();
                int nextId = NextIdAgendamento(agenda);
                agenda.Add(new Agendamento
                {
                    Id = nextId,
                    ServicoSolicitado =
                            new ServicoSolicitado
                            {
                                Id = nextId,
                                Servico = meusServicos.Servicos.First(),
                                Funcionario = meusFuncionarios.Funcionarios.First(f => f.Servicos.Contains(meusServicos.Servicos.First())),
                                Preco = meusServicos.Servicos.First().Preco

                            },
                    DtAgendamento = new DateTime(2021, 1, 29, 12, 0, 0)
                }); ;

                nextId = NextIdAgendamento(agenda);
                agenda.Add(new Agendamento
                {
                    Id = nextId,
                    ServicoSolicitado =
                            new ServicoSolicitado
                            {
                                Id = nextId,
                                Servico = meusServicos.Servicos.First(),
                                Funcionario = meusFuncionarios.Funcionarios.First(f => f.Servicos.Contains(meusServicos.Servicos.First())),
                                Preco = meusServicos.Servicos.First().Preco
                            },
                    DtAgendamento = new DateTime(2021, 1, 29, 11, 0, 0),
                    Status = Agendamento.StatusAgenda.CanceladoPeloCliente
                });


                Agendamento agendamento1 = new Agendamento();
                nextId = NextIdAgendamento(agenda);
                agendamento1.IncluirAgendamento(nextId, meusClientes.Clientes.First(),
                    new ServicoSolicitado
                    {
                        Id = nextId,
                        Servico = meusServicos.Servicos.First(),
                        Funcionario = meusFuncionarios.Funcionarios.First(f => f.Servicos.Contains(meusServicos.Servicos.First())),
                        Preco = meusServicos.Servicos.First().Preco
                    }, new DateTime(2021, 1, 30, 10, 0, 0),
                    agenda);

                agenda.Add(agendamento1);

                Agendamento agendamento2 = new Agendamento();
                nextId = NextIdAgendamento(agenda);
                agendamento2.IncluirAgendamento(nextId, meusClientes.Clientes[1],
                    new ServicoSolicitado
                    {
                        Id = nextId,
                        Servico = meusServicos.Servicos[1],
                        Funcionario = meusFuncionarios.Funcionarios.First(f => f.Servicos.Contains(meusServicos.Servicos[1])),
                        Preco = meusServicos.Servicos.First().Preco
                    }, new DateTime(2021, 1, 30, 10, 0, 0),
                    agenda);

                agenda.Add(agendamento2);


                Faturamento(agenda, new DateTime(2021, 1, 29, 0, 0, 0), new DateTime(2021, 1, 29, 23, 59, 59));
                Console.WriteLine("");
                Console.WriteLine("");
                Faturamento(agenda, new DateTime(2021, 1, 1, 0, 0, 0), new DateTime(2021, 1, 31, 23, 59, 59));

            }
            catch (IOException)
            {
                Console.WriteLine("Ocorreu um erro. Tente novamente mais tarde. ");
            }
            catch (ArgumentNullException nrEx)
            {
                Console.WriteLine("Aqui caiu a Null Reference!");
                throw;
            }
            catch (Exception)
            {
                Console.WriteLine("Deu ruim!!!");
                //throw;
            }
            Console.WriteLine("Continuando...");
            Console.ReadLine();
        }

        private static int NextIdAgendamento(List<Agendamento> agenda)
        {

            if (agenda.Any())
                return agenda.Max(a => a.Id) + 1;
            else
                return 1;

        }

        private static void Faturamento(List<Agendamento> agenda, DateTime periodoInical, DateTime periodoFinal)
        {
            //cria uma lista retirando os cancelados
            List<Agendamento> agendamentosEfetuados = agenda.FindAll(a => a.Status != StatusAgenda.CanceladoPeloSalao &&
                                                   a.Status != StatusAgenda.CanceladoPeloCliente &&
                                                   a.DtAgendamento >= periodoInical && a.DtAgendamento <= periodoFinal);


            //cria Dictionary para acumular o valor total de servico em cada funcionario
            Dictionary<Funcionario, decimal> valorFuncionarioTotal = new Dictionary<Funcionario, decimal>();
            foreach (Agendamento agendamentoEfetuado in agendamentosEfetuados)
            {
                if (!valorFuncionarioTotal.ContainsKey(agendamentoEfetuado.ServicoSolicitado.Funcionario))
                {
                    valorFuncionarioTotal.Add(agendamentoEfetuado.ServicoSolicitado.Funcionario, 0);
                }
                //soma o valor do servico
                valorFuncionarioTotal[agendamentoEfetuado.ServicoSolicitado.Funcionario] += agendamentoEfetuado.ServicoSolicitado.Preco;

            }

            decimal valorTotalSalao = 0;
            decimal valorTotalComissao = 0;
            Console.WriteLine("");
            Console.WriteLine($"Comissões do periodo {periodoInical} a {periodoFinal}");
            Console.WriteLine("");
            foreach (var funcionario in valorFuncionarioTotal)
            {
                Console.WriteLine($"Comissão de { funcionario.Key.Nome} é de : {(funcionario.Value * 30 / 100):C2}");
                valorTotalSalao += funcionario.Value;
                valorTotalComissao += (funcionario.Value * 30 / 100);
            }

            Console.WriteLine("");
            Console.WriteLine($"Resumo de faturamento do periodo {periodoInical} a {periodoFinal}");
            Console.WriteLine("");
            Console.WriteLine($"==========================================================");
            Console.WriteLine($"Total Faturamento: { valorTotalSalao:C2}");
            Console.WriteLine($"Total Comissões: { valorTotalComissao:C2}");
            Console.WriteLine($"Total Faturamento: { (valorTotalSalao - valorTotalComissao):C2}");
            Console.WriteLine($"==========================================================");

        }

        static MinhaBaseClientes IncluirMeusClientes()
        {
            Cliente c1 = new Cliente();
            c1.Incluir(1, "Thamirys", "999999999", "12345678901");

            Cliente c2 = new Cliente();
            c2.Incluir(2, "Thaise", "999999998", "12345678902");

            MinhaBaseClientes mc = new MinhaBaseClientes();
            mc.Incluir(c1);
            mc.Incluir(c2);

            var c3 = new Cliente();
            c3.Incluir(3, "Maria", "999999997", "12345678903");

            var c4 = new Cliente();
            c4.Incluir(4, "Joana", "999999996", "12345678904");

            mc.IncluirLista(c3, c4);

            return mc;
        }

        static MinhaBaseServicos IncluirMeusServicos()
        {
            Servico s1 = new Servico();
            s1.Incluir(1, "Corte de Cabelo", 59, 130);

            Servico s2 = new Servico();
            s2.Incluir(2, "Manicure", 59, 20);

            Servico s3 = new Servico();
            s3.Incluir(3, "Pedicure", 59, 30);

            Servico s4 = new Servico();
            s4.Incluir(4, "Limpeza de pele", 59, 100);

            MinhaBaseServicos bs = new MinhaBaseServicos();
            bs.Incluir(s1);
            bs.Incluir(s2);
            bs.Incluir(s3);
            bs.Incluir(s4);



            return bs;
        }

        static MinhaBaseFuncionarios IncluirFuncionarios(MinhaBaseServicos baseDeServico)
        {
            Funcionario f1 = new Funcionario();
            Endereco e1 = new Endereco();
            e1.Incluir(1, "Rua dos bobos", "12345-010", "Vila dos Devs", "São Paulo", "SP", "0", string.Empty);

            f1.Incluir("Maria", "999999999", e1, Funcionario.CargoFunc.Cabelereira);

            Funcionario f2 = new Funcionario();
            f2.Incluir("Rosana", "999999998", e1, Funcionario.CargoFunc.Manicure);

            Funcionario f3 = new Funcionario();
            f3.Incluir("Joana", "999999997", e1, Funcionario.CargoFunc.Esteticista);

            MinhaBaseFuncionarios bf = new MinhaBaseFuncionarios();
            bf.Incluir(f1);
            bf.Incluir(f2);
            bf.Incluir(f3);


            bf.IncluirServicoDeUmFuncionario(1, baseDeServico.Servicos.FirstOrDefault(x => x.Id == 1));
            bf.IncluirServicoDeUmFuncionario(2, baseDeServico.Servicos.FirstOrDefault(x => x.Id == 2));
            bf.IncluirServicoDeUmFuncionario(2, baseDeServico.Servicos.FirstOrDefault(x => x.Id == 3));
            bf.IncluirServicoDeUmFuncionario(3, baseDeServico.Servicos.FirstOrDefault(x => x.Id == 4));

            return bf;
        }

        static void ChamarOExcluir()
        {

        }
    }
}
