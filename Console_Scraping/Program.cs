namespace Console_Scraping
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("Iniciando serviço de busca dos cursos Alura...");
            Console.WriteLine("=================================================");
            CoursesService.Execute(args);
            Console.WriteLine("=================================================");
            Console.WriteLine("Finalizado serviço de busca dos cursos Alura...");
            Console.WriteLine("=================================================");
        }
    }
}
