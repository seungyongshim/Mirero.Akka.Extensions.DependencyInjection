namespace SeungyongShim.Akka.Extensions.DependencyInjection
{
    using global::Akka.Actor;

    public class DIExtension2 : ExtensionIdProvider<DIExt2>
    {
        public static DIExtension2 DIExtensionProvider { get; set; } = new DIExtension2();

        public override DIExt2 CreateExtension(ExtendedActorSystem system)
        {
            var extension = new DIExt2();
            return extension;
        }
    }
}
