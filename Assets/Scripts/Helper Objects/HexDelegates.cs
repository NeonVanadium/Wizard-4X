/// <summary>
/// This class wraps several delegates for events that trigger upon
/// certain conditions down at the Hex level, but must be handled in
/// the GameMaster.
/// 
/// Created by the GameMaster and passed down through the board to the hexes.
/// </summary>
public class HexDelegates
{
    public delegate void HexClickDelegate(int row, int col);

    public HexClickDelegate onClick;

    public delegate void PlayerSightedDelegate(Player player);

    public PlayerSightedDelegate playerSighted;
}