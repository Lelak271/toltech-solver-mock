using Toltech.Solver.Contracts;

namespace Toltech.Solver.Mock
{
    /// <summary>
    /// Implémentation fictive du moteur de calcul.
    ///
    /// Cette classe permet à l'application publique de fonctionner
    /// sans exposer ni dépendre du véritable moteur Toltech.Solver.
    /// </summary>
    public sealed class MockComputeEngine : IComputeEngine
    {
        // Générateur aléatoire utilisé pour produire les résultats fictifs.
        private readonly Random _random = new();

        // Limites des valeurs d'influence fictives.
        private const double MinInfluence = -4.0;
        private const double MaxInfluence = 4.0;

        /// <summary>
        /// Effectue un calcul fictif à partir des exigences fournies.
        /// </summary>
        public Task<ComputeResult> ComputeAsync(ComputeRequest request)
        {
            var result = new ComputeResult
            {
                IsSuccess = true
            };

            // Génère un résultat fictif pour chaque exigence demandée.
            foreach (var requirement in request.Requirements)
            {
                result.RawResults[requirement.Id] = CreateFakeSolveResult(request, requirement);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Génère un résultat fictif pour une exigence.
        /// </summary>
        private SolveResults CreateFakeSolveResult(
    ComputeRequest request,
    ComputeRequirement requirement)
        {
            var result = new SolveResults();

            // Parcourt uniquement les liaisons actives du modèle.
            foreach (var linkage in request.ModelData.Where(x => x.Active))
            {
                // Détermine les degrés de liberté représentés
                // par cette liaison.
                foreach (var type in GetUnknownTypes(linkage))
                {
                    // Les translations utilisent une influence plus faible
                    // que les rotations dans les résultats fictifs.
                    double factor = type == UnknownType.Tu
                                 || type == UnknownType.Tv
                                 || type == UnknownType.Tw
                        ? 1.0
                        : 500.0;

                    result.Details.Add(new DecompositionResults
                    {
                        // Plusieurs résultats peuvent avoir le même IdData :
                        // chacun correspond alors à un UnknownType différent.
                        IdData = linkage.Id,

                        Type = type,

                        // Valeurs fictives utilisées uniquement pour
                        // alimenter l'interface avec des résultats réalistes.
                        InflX = factor * GenerateGaussianInfluence(),
                        InflY = factor * GenerateGaussianInfluence(),
                        InflZ = factor * GenerateGaussianInfluence()
                    });
                }
            }

            return result;
        }

        private static IEnumerable<UnknownType> GetUnknownTypes(
            ComputeLinkage linkage)
        {
            return linkage.Linkage switch
            {
                LinkageType.PointContact =>
                [
                    UnknownType.Tu
                ],

                LinkageType.LinearContact =>
                [
                    UnknownType.RotC,
            UnknownType.Tu
                ],

                LinkageType.AnnularContact =>
                [
                    UnknownType.Tu,
            UnknownType.Tv
                ],

                LinkageType.PlanarContact =>
                [
                    UnknownType.RotA,
            UnknownType.RotB,
            UnknownType.RotC,
            UnknownType.Tu
                ],

                LinkageType.RevoluteContact =>
                [
                    UnknownType.RotB,
            UnknownType.RotC,
            UnknownType.Tu,
            UnknownType.Tv,
            UnknownType.Tw
                ],

                LinkageType.PrismaticContact =>
                [
                    UnknownType.RotA,
            UnknownType.RotB,
            UnknownType.RotC,
            UnknownType.Tu,
            UnknownType.Tv
                ],

                LinkageType.CylindricalContact =>
                [
                    UnknownType.RotA,
            UnknownType.RotB,
            UnknownType.Tu,
            UnknownType.Tv,
            UnknownType.Tw
                ],

                LinkageType.SphericalContact =>
                [
                    UnknownType.Tu,
            UnknownType.Tv,
            UnknownType.Tw
                ],

                LinkageType.FixedContact =>
                [
                    UnknownType.RotA,
            UnknownType.RotB,
            UnknownType.RotC,
            UnknownType.Tu,
            UnknownType.Tv,
            UnknownType.Tw
                ],

                _ => []
            };
        }


        /// <summary>
        /// Génère une valeur suivant une loi normale centrée sur 0.
        ///
        /// La valeur est ensuite limitée à l'intervalle [-1 ; +1]
        /// afin de conserver des résultats fictifs facilement lisibles.
        /// </summary>
        private double GenerateGaussianInfluence()
        {
            // Méthode de Box-Muller :
            // transformation de deux variables uniformes en une
            // variable suivant approximativement une loi normale.
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();

            double gaussian =
                Math.Sqrt(-2.0 * Math.Log(u1))
                * Math.Cos(2.0 * Math.PI * u2);

            // Écart-type volontairement faible pour que la majorité
            // des résultats reste proche de zéro.
            const double standardDeviation = 1;

            double value = gaussian * standardDeviation;

            // Limitation de la valeur
            return Math.Clamp(value, MinInfluence, MaxInfluence);
        }


        /// <summary>
        /// Vérification fictive de l'isostatisme d'une pièce.
        ///
        /// Le Mock ne réalise aucun calcul mécanique réel.
        /// </summary>
        public Task<bool> IsPartIsostaticAsync(
            List<ComputeLinkage> modelData,
            ComputePart part)
        {
            // Une pièce fixée est considérée comme isostatique.
            if (part.IsFixed)
                return Task.FromResult(true);

            // Résultat fictif.
            return Task.FromResult(true);
        }

        /// <summary>
        /// Vérification fictive de l'isostatisme du modèle.
        ///
        /// Le Mock ne réalise aucun calcul mécanique réel.
        /// </summary>
        public Task<bool> IsModelIsostaticAsync(
            List<ComputeLinkage> linkages,
            int partId1,
            int partId2,
            int idFixPart = 0)
        {
            // Résultat fictif permettant à l'application
            // de poursuivre son fonctionnement normal.
            return Task.FromResult(true);
        }
    }



}