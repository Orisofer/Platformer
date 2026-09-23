namespace OriGame.Player
{
    public interface IAbilityResolver
    {
        public ref readonly ResolvedMovement ResolveMovement(PlayerMovementRequest[] requests);
    }
}

