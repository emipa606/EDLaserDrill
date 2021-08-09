namespace Jaxxa.EnhancedDevelopment.Core.Comp.Interface
{
    // Token: 0x02000002 RID: 2
    public interface IRequiresShipResources
    {
        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1
        bool Satisfied { get; }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000002 RID: 2
        string StatusString { get; }

        // Token: 0x06000003 RID: 3
        bool UseResources();
    }
}