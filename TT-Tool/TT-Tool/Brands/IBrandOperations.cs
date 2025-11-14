namespace TT_Tool.Brands
{
    /// <summary>
    /// Interfaz base para operaciones específicas de cada marca
    /// </summary>
    public interface IBrandOperations
    {
        /// <summary>
        /// Nombre de la marca
        /// </summary>
        string BrandName { get; }

        /// <summary>
        /// Obtener el panel de operaciones para esta marca
        /// </summary>
        Panel GetOperationsPanel();

        /// <summary>
        /// Inicializar operaciones de la marca
        /// </summary>
        void Initialize();
    }
}


