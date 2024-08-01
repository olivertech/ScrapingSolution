namespace Console_Scraping.Services.Alura
{
    public static class AluraService
    {
        private static ServiceProvider _serviceProvider = null!;

        /// <summary>
        /// Método principal do serviço
        /// </summary>
        /// <param name="args"></param>
        public static void Execute(string[] args)
        {
            bool clearAll = true;

            _serviceProvider = Dependencies.Injections();
            string? chromePath = ConfigurationManager.AppSettings["chromepath"];
            IWebDriver driver = new ChromeDriver(chromePath);

            try
            {
                var serviceProvider = _serviceProvider.GetService<IServiceAlura>()!;

#if DEBUG
                args = new string[1];
                args[0] = "IA";
#endif
                if (args[0] is null || args.Length != 1)
                {
                    Console.WriteLine("Informe 1 argumento de pesquisa!");
                    Console.ReadKey();
                    driver.Quit();
                    Environment.Exit(0);
                }

                driver.Navigate().GoToUrl("https://www.alura.com.br/");

                //Para maximizar o navegador
                driver.Manage().Window.Maximize();

                // Localizando o campo de pesquisa pelo nome 'query' no site Alura,
                // e realizando a pesquisa pelo termo usado na pesquisa
                IWebElement element = driver.FindElement(By.Name("query"));
                element.Clear();
                element.SendKeys(args[0]);
                element.Submit();

                //Aguardar que a consulta seja realizada
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

                //Recupero os botões de paginação
                IReadOnlyList<IWebElement> paginationLinks = driver.FindElements(By.ClassName("paginationLink"));

                int totalPages = 0;

                //Recupero o número total de páginas
                if (paginationLinks.Count == 5)
                {
                    int paginaLocationStartPosition = paginationLinks[4]!.GetAttribute("href").ToString()!.IndexOf("pagina=") + 7;
                    int paginaLocationEndPosition = paginationLinks[4]!.GetAttribute("href").ToString()!.IndexOf("&query=");

                    totalPages = int.Parse(paginationLinks[4]!.GetAttribute("href").ToString()!.Substring(paginaLocationStartPosition, paginaLocationEndPosition - paginaLocationStartPosition));
                }

                var link = paginationLinks[0].GetAttribute("href");
                string[] parts = link.Split(['?', '&']);

                if (clearAll)
                {
                    //Remove todos os registros já carregados previamente
                    DeleteAllContentsDocuments(serviceProvider!);

                    //Percorro cada um dos botões de paginação, clicando um a um
                    for (int i = 1; i <= totalPages; i++)
                    {
                        //Recupero os 25 conteúdos presentes na tela
                        GetContents(driver, serviceProvider);

                        //Monta a pagina a ser buscada
                        parts[1] = parts[1].Replace(i.ToString(), "");

                        //Monto o link da próxima paágina de conteúdos
                        var buildedLink = parts[0] + "?" + parts[1] + (i + 1).ToString() + "&" + parts[2];

                        //Navego pra nova página de conteúdo
                        driver.Navigate().GoToUrl(buildedLink);

                        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

                        PrintCount(serviceProvider);
                    }
                }

                //Recupero os demais conteúdos
                GetMoreContents(driver, serviceProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Não foi possível fazer o scraping para o termo pesquisado... Aguarde e refaça a atividade, trocando o termo de pesquisa, caso necessário - Erro: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("============================================================================================");
                Console.WriteLine("Scraping concluído com sucesso! Pressione qualquer tecla para fechar essa janela...");
                Console.WriteLine("============================================================================================");
                Console.ReadKey();
                // Close the browser
                driver.Quit();
            }
        }

        /// <summary>
        /// Método que varre a tela e recupera todos os conteúdos listados,
        /// armazenando os dados iniciais de cada um
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="serviceProvider"></param>
        private static void GetContents(IWebDriver driver, IServiceAlura serviceProvider)
        {
            IReadOnlyList<IWebElement> titles = driver.FindElements(By.ClassName("busca-resultado-nome"));
            IReadOnlyList<IWebElement> descriptions = driver.FindElements(By.ClassName("busca-resultado-descricao"));
            IReadOnlyList<IWebElement> link = driver.FindElements(By.ClassName("busca-resultado-link"));

            //Recupera todos os links dos conteúdos
            for (int i = 0; i < titles.Count; i++)
            {
                var newContent = new AluraContent
                {
                    Title = titles[i].Text,
                    Description = descriptions[i].Text,
                    Link = link[i].GetAttribute("href")
                };

                serviceProvider.AddAsync(newContent);
            }
        }

        /// <summary>
        /// Método que faz a busca dentro de cada conteúdo, pra
        /// recuperar o nome dos professores e a carga horária
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="serviceProvider"></param>
        private static void GetMoreContents(IWebDriver driver, IServiceAlura serviceProvider)
        {
            var contents = serviceProvider.GetAllAsync();

            foreach (var content in contents)
            {
                driver.Navigate().GoToUrl(content.Link);

                IWebElement cargaHoraria = null!;
                IReadOnlyList<IWebElement> professores = null!;

                try
                {
                    cargaHoraria = driver.FindElement(By.ClassName("formacao__info-destaque"));
                    content.Duration = cargaHoraria.Text;
                }
                catch (Exception)
                {
                    Console.WriteLine("Link sem a tag 'formacao__info-destaque'");
                }

                try
                {
                    cargaHoraria = driver.FindElement(By.ClassName("courseInfo-card-wrapper-infos"));
                    content.Duration = cargaHoraria.Text;
                }
                catch (Exception)
                {
                    Console.WriteLine("Link sem a tag 'courseInfo-card-wrapper-infos'");
                }

                try
                {
                    professores = driver.FindElements(By.ClassName("formacao-instrutor-nome"));

                    if (professores.Count() > 0)
                    {

                        string nomes = null!;
                        for (int i = 0; i < professores.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(professores[i].Text))
                                nomes += professores[i].Text + "|";
                        }
                        content.Professores = nomes[..^1];
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Link sem a tag 'formacao-instrutor-nome'");
                }

                try
                {
                    professores = driver.FindElements(By.ClassName("instructor-title--name"));

                    if (professores.Count() > 0)
                    {
                        string nomes = null!;
                        for (int i = 0; i < professores.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(professores[i].Text))
                                nomes += professores[i].Text + "|";
                        }
                        content.Professores = nomes[..^1];
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Link sem a tag 'instructor-title--name'");
                }

                serviceProvider.UpdateAsync(content);
            }
        }

        /// <summary>
        /// Método que recupera o total parcial de conteúdos recuperados
        /// </summary>
        /// <param name="serviceProvider"></param>
        private async static void PrintCount(IServiceAlura serviceProvider)
        {
            long total = await serviceProvider.GetCountAsync();
            Console.WriteLine($"Total parcial de documentos : {total}"); ;
        }

        /// <summary>
        /// Método que apaga todos os conteúdos previamente gravados no banco
        /// </summary>
        /// <param name="serviceProvider"></param>
        private async static void DeleteAllContentsDocuments(IServiceAlura serviceProvider)
        {
            var filter = new BsonDocument();
            var result = await serviceProvider.DeleteAllAsync();
            Console.WriteLine($"{result.DeletedCount} documentos apagados...");
        }
    }
}
