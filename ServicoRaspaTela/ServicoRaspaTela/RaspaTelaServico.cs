using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RaspaTela.Dados.Models;


namespace RaspaTela.Servico
{
    public class RaspaTelaServico : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IConfiguration _configuration;
        private IWebDriver _driver;

        public RaspaTelaServico(IConfiguration configuration)
        {
            _configuration = configuration;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");

            _driver = new ChromeDriver(
                _configuration["chromedriver.exe"],
                chromeOptions);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                //Inicializar abaixo Services/Providers caso haja

                _timer = new Timer((e) => Executar(), null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(1));

                InserirLog($"Serviço Iniciado!");
            }
            catch (Exception ex)
            {
                GravarLogErro($"Erro StartAsync Serviço: {ex.Message}");
                _timer.Dispose();
            }

            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _timer.Change(Timeout.Infinite, 0);
                _timer.Dispose();

                InserirLog($"Serviço Parado!");
            }
            catch (Exception ex)
            {
                GravarLogErro($"Erro StopAsync Serviço: {ex.Message}");
                _timer?.Dispose();
            }

            return Task.CompletedTask;
        }

        public void Executar()
        {
            _timer.Change(Timeout.Infinite, 0); //Para o tempo

            try
            {
                using var context = new DBContext();

                //Listar dados BD - SELECT
                var moedas = context.TB_Moeda_Estrangeira.ToList();

                CarregarPagina();

                var crawlerCotacao = ObterCotacoesMoedasEstrangeiras();

                if (moedas.Count > 0 && crawlerCotacao != null && !moedas.Equals(crawlerCotacao))
                {

                    foreach (var c in crawlerCotacao) 
                    {
                        if (possuiMoeda(c.Descricao))
                        {
                            var capturarMoedaNoBD = context.TB_Moeda_Estrangeira.Where(d => d.Descricao == c.Descricao).ToList();

                            //Atualizar dados BD - UPDATE
                            var updateMoedas = context.TB_Moeda_Estrangeira.Find(capturarMoedaNoBD.FirstOrDefault().ID);

                            if (updateMoedas.Valor != c.Valor)
                            {
                                updateMoedas.Valor = c.Valor;
                                updateMoedas.Data_Inclusao = DateTime.Now;

                                context.SaveChanges();

                                InserirLog($"Moeda {c.Descricao} atualizada com sucesso");
                            }

                        }
                        else
                        {
                            //Inserir dados BD - INSERT
                            var insertMoedas = new MoedaEstrangeira()
                            {
                                Descricao = c.Descricao,
                                Valor = c.Valor,
                                Data_Inclusao = DateTime.Now
                            };

                            context.TB_Moeda_Estrangeira.Add(insertMoedas);
                            context.SaveChanges();

                            InserirLog($"Moeda {insertMoedas.Descricao} inserida com sucesso");
                        }
                    }
                }

                //Deletar dados DB - DELETE
                //var entityToDelete = context.TB_Moeda_Estrangeira.FirstOrDefault(m => m.ID == 9);
                //context.TB_Moeda_Estrangeira.Remove(entityToDelete);
                //context.SaveChanges();

            }
            catch (Exception ex)
            {
                GravarLogErro($"Erro ao executar serviço: {ex.Message}"); 
            }
            finally
            {
                _timer?.Change(TimeSpan.FromMinutes(1), TimeSpan.Zero); //reinicia depois de 1 minuto
            }
        }

        #region Métodos WebDriver
        public void CarregarPagina()
        {
            _driver.Manage().Timeouts().PageLoad =
                TimeSpan.FromSeconds(60);
            _driver.Navigate().GoToUrl("https://br.investing.com/currencies/exchange-rates-table");
        }

        public List<MoedaEstrangeira> ObterCotacoesMoedasEstrangeiras()
        {
            var cotacoes = new List<MoedaEstrangeira>();

            var rowsCotacoes = _driver
                .FindElement(By.Id("exchange_rates_1"))
                .FindElement(By.TagName("tbody"))
                .FindElements(By.TagName("tr"));

            foreach (var rowCotacao in rowsCotacoes)
            {
                var dadosCotacao = rowCotacao.FindElements(
                    By.TagName("td"));

                string valorMoeda = dadosCotacao[1].Text;

                var cotacao = new MoedaEstrangeira();
                cotacao.Descricao = dadosCotacao[0].Text;
                cotacao.Valor = valorMoeda.Length > 1 ? valorMoeda.Substring(0, 4) : valorMoeda;
                cotacao.Data_Inclusao = DateTime.Now;

                cotacoes.Add(cotacao);
            }

            return cotacoes;
        }

        public void Fechar()
        {
            _driver.Quit();
            _driver = null;
        }
        #endregion

        #region Métodos banco de dados
        public void InserirLog(string descricao)
        {
            try
            {
                using var context = new DBContext();

                var insertLog = new LogServico()
                {
                    Descricao = descricao,
                    Data_Log = DateTime.Now
                };

                context.TB_Log_Servico.Add(insertLog);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                GravarLogErro($"Erro ao InserirLog: {ex.Message}");
            }
        }

        public void GravarLogErro(string descricao)
        {
            try
            {
                using var context = new DBContext();

                var insertLog = new LogErro()
                {
                    Descricao = descricao,
                    Data_Log_Erro = DateTime.Now
                };

                context.TB_Log_Erro.Add(insertLog);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool possuiMoeda(string descricao)
        {
            using var context = new DBContext();

            var moedas = context.TB_Moeda_Estrangeira.Where(d => d.Descricao == descricao).ToList();

            if (moedas.Count() > 0)
                return true;
            else
                return false;
        }
        #endregion

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
