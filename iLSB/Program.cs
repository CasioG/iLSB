using iLSB.Utils;
using Microsoft.Extensions.Configuration;

namespace iLSB
{
    class Program
    {
        private static int PRISE;
        private static double TRONCONNAGE;
        private static double FACAGE;
        private static int LONGUEUR_STD;
        private static IClipboard _clipboard;

        static void Main(string[] args)
        {
            double longueurBarre;
            double longueurPiece;
            int listeParametres = args.Length;
            string erreurMessage = "L'information passé en paramètre n'est pas valide";

            LireEtAppliquerConfiguration();

            _clipboard = SetClipboardBasedOnOs();

            switch (listeParametres)
            {
                case 1:
                    try
                    {
                        longueurPiece = ObtenirLongueurPieceEnPouces(args[0]);
                        longueurBarre = LONGUEUR_STD;
                        RealiserCalculEtEnvoyerClipboard(longueurBarre, longueurPiece);
                    }
                    catch (Exception e)
                    {
                        _clipboard.SetText(erreurMessage);
                        Console.WriteLine(erreurMessage);
                        break;
                    }

                    break;
                case 2:
                    try
                    {
                        longueurPiece = ObtenirLongueurPieceEnPouces(args[0]);
                        longueurBarre = double.Parse(args[1]);
                        RealiserCalculEtEnvoyerClipboard(longueurBarre, longueurPiece);
                    }
                    catch (Exception e)
                    {
                        _clipboard.SetText(erreurMessage);
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

        private static IClipboard SetClipboardBasedOnOs()
        {
            if (OperatingSystem.IsWindows())
                return new ClipboardWindows();
            else
                return new ClipboardLinux();
        }

        public static void RealiserCalculEtEnvoyerClipboard(double barre, double piece)
        {
            piece = piece + TRONCONNAGE + FACAGE;

            int[] piecesParBarre = new int[4];
            for (int i = 0; i < piecesParBarre.Length; i++)
            {
                int quantiteParBarre = (int)Math.Floor((barre * (4 - i) / 4 - PRISE) / piece);
                if (quantiteParBarre <= 0)
                    quantiteParBarre = 0;
                piecesParBarre[i] = quantiteParBarre;
            }

            double[] dimensionBarreOpmizer = new double[4];
            for (int i = 0; i < dimensionBarreOpmizer.Length; i++)
            {
                dimensionBarreOpmizer[i] =
                    Math.Round(((piecesParBarre[i] * piece) + PRISE) * 8, MidpointRounding.AwayFromZero) / 8;
            }

            //Partie std
            string result = $"****** RESULTAT STANDARD ******\n";
            for (int i = 0; i < piecesParBarre.Length; i++)
            {
                if (piecesParBarre[i] > 0)
                    result += $"Barre de {barre * (4 - i) / 4} pouces\tfait {piecesParBarre[i]} pièces\n";
            }

            //Partie optimizé
            result += $"\n****** RESULTAT OPTIMIZER ******\n";
            for (int i = 0; i < dimensionBarreOpmizer.Length; i++)
            {
                if (piecesParBarre[i] > 0)
                    result += $"Barre de {dimensionBarreOpmizer[i]} pouces\tfait {piecesParBarre[i]} pièces\n";
            }

            result += $"\nFormule: ( quantité a produire x {piece} ) + {PRISE}";

            _clipboard.SetText(result);
            Console.WriteLine(result);
        }

        public static double GetInputUtilisateur(string info)
        {
            double facteurMetriquePouce = 1;
            double inputUtilisateur;
            bool piece = false;
            string input;

            if (info == "pièce")
                piece = true;

            do
            {
                Console.Write($"Saisir la dimension de la {info} en pouces: ");
                input = Console.ReadLine();

                if (piece)
                {
                    if (input.Contains("mm"))
                    {
                        input = input.Replace("mm", "").Trim();
                        facteurMetriquePouce = 1 / 25.4;
                    }
                    else
                        input = input.Trim();
                }
            } while (!double.TryParse(input, out inputUtilisateur));

            return inputUtilisateur * facteurMetriquePouce;
        }

        public static void LireEtAppliquerConfiguration()
        {
            string pathFichierConfig = Directory.GetCurrentDirectory();
            string fichierConfig = "appsettings.json";
            string cheminComplet = Path.Combine(pathFichierConfig, fichierConfig);

            if (!File.Exists(cheminComplet))
            {
                Directory.CreateDirectory(pathFichierConfig);
                File.WriteAllText(cheminComplet,
                    "{\r\n  \"prise\": \"2\",\r\n  \"tronconnage\": \"0.125\",\r\n  \"facage\": \"0.03\",\r\n  \"longueur_std\": \"36\"\r\n}");
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(pathFichierConfig)
                .AddJsonFile("appsettings.json", false)
                .Build();

            PRISE = Convert.ToInt16(configuration["prise"]);
            TRONCONNAGE = Convert.ToDouble(configuration["tronconnage"]);
            FACAGE = Convert.ToDouble(configuration["facage"]);
            LONGUEUR_STD = Convert.ToInt16(configuration["longueur_std"]);
        }

        public static double ObtenirLongueurPieceEnPouces(string s_piece)
        {
            double facteurMetriquePouce;
            double piece;
            if (s_piece.Contains("mm"))
            {
                s_piece = s_piece.Replace("mm", "").Trim();
                facteurMetriquePouce = 1 / 25.4;
            }
            else
            {
                s_piece = s_piece.Trim();
                facteurMetriquePouce = 1;
            }

            try
            {
                piece = double.Parse(s_piece) * facteurMetriquePouce;
            }
            catch (Exception e)
            {
                throw;
            }

            return piece;
        }
    }
}
