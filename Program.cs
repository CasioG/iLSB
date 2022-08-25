using Microsoft.Extensions.Configuration;
using System;
using System.Windows.Forms;

namespace iLSB
{
    class Program
    {
        private static int PRISE;
        private static double TRONCONNAGE;
        private static double FACAGE;
        private static int LONGUEUR_STD;

        [STAThread]
        static void Main(string[] args)
        {
            double longueurBarre;
            double longueurPiece;
            int listeParametres = args.Length;
            string erreurMessage = "L'information passé en paramètre n'est pas valide";

            LireEtAppliquerConfiguration();

            switch (listeParametres)
            {
                case 1:
                    try
                    {
                        longueurPiece = double.Parse(args[0]);
                        longueurBarre = LONGUEUR_STD;
                        RealiserCalculEtEnvoyerClipboard(longueurBarre, longueurPiece);
                    }
                    catch (Exception e)
                    {
                        Clipboard.SetText(erreurMessage);
                        Console.WriteLine(erreurMessage);
                        break;
                    }
                    break;
                case 2:
                    try
                    {
                        longueurPiece = double.Parse(args[0]);
                        longueurBarre = double.Parse(args[1]);
                        RealiserCalculEtEnvoyerClipboard(longueurBarre, longueurPiece);
                    }
                    catch (Exception e)
                    {
                        Clipboard.SetText(erreurMessage);
                        Console.WriteLine(erreurMessage);
                        break;
                    }
                    break;
                default:
                    longueurPiece = GetInputUtilisateur("pièce");
                    longueurBarre = GetInputUtilisateur("barre");
                    RealiserCalculEtEnvoyerClipboard(longueurBarre, longueurPiece);
                    break;
            }
        }

        public static void RealiserCalculEtEnvoyerClipboard(double barre, double piece)
        {
            piece = piece + TRONCONNAGE + FACAGE;
            int piecesParBarre = (int)Math.Floor((barre - PRISE) / piece);
            int piecesParTroisQuartsBarre = (int)Math.Floor((barre * 3 / 4 - PRISE) / piece);
            int piecesDemiBarre = (int)Math.Floor((barre * 1 / 2 - PRISE) / piece);
            int piecesUnQuartBarre = (int)Math.Floor((barre * 1 / 4 - PRISE) / piece);
            double troisQuartDeBarreOptimize = Math.Round(((piecesParTroisQuartsBarre * piece) + PRISE) * 8, MidpointRounding.AwayFromZero) / 8;
            double demiBarreOptimize = Math.Round(((piecesDemiBarre * piece) + PRISE) * 8, MidpointRounding.AwayFromZero) / 8;
            double quartDeBarreOptimize = Math.Round(((piecesUnQuartBarre * piece) + PRISE) * 8, MidpointRounding.AwayFromZero) / 8;

            //Partie std
            string result = $"****** RESULTAT STANDARD ******\n" +
                $"Barre de {barre} pouces\tfait {piecesParBarre} pièces\n" +
                $"Barre de {barre * 3 / 4} pouces\tfait {piecesParTroisQuartsBarre} pièces\n" +
                $"Barre de {barre * 1 / 2} pouces\tfait {piecesDemiBarre} pièces\n" +
                $"Barre de {barre * 1 / 4} pouces\tfait {piecesUnQuartBarre} pièces\n\n";

            //Partie optimizé
            result += $"****** RESULTAT OPTIMIZER ******\n" +
                $"Barre de {barre} pouces\tfait {piecesParBarre} pièces\n" +
                $"Barre de {troisQuartDeBarreOptimize} pouces\tfait {piecesParTroisQuartsBarre} pièces\n" +
                $"Barre de {demiBarreOptimize} pouces\tfait {piecesDemiBarre} pièces\n" +
                $"Barre de {quartDeBarreOptimize} pouces\tfait {piecesUnQuartBarre} pièces\n\n";

            result += $"Formule: ( quantité a produire x {piece} ) + {PRISE}";

            Clipboard.SetText(result);
            Console.WriteLine(result);
        }

        public static double GetInputUtilisateur(string info)
        {
            double inputUtilisateur;
            do
            {
                Console.Write($"Saisir la dimension de la {info} en pouces: ");
            } while (!double.TryParse(Console.ReadLine(), out inputUtilisateur));
            return inputUtilisateur;
        }

        public static void LireEtAppliquerConfiguration()
        {
            string pathExecutable = Directory.GetParent(AppContext.BaseDirectory).FullName;
            string fichierConfiguration = "appsettings.json";
            string cheminComplet = Path.Combine(pathExecutable, fichierConfiguration);

            if(!File.Exists(cheminComplet))
            {
                File.WriteAllText(cheminComplet, "{\r\n  \"prise\": \"2\",\r\n  \"tronconnage\": \"0.125\",\r\n  \"facage\": \"0.03\",\r\n  \"longueur_std\": \"36\"\r\n}");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            PRISE = Convert.ToInt16(configuration["prise"]);
            TRONCONNAGE = Convert.ToDouble(configuration["tronconnage"]);
            FACAGE = Convert.ToDouble(configuration["facage"]);
            LONGUEUR_STD = Convert.ToInt16(configuration["longueur_std"]);
        }
    }
}