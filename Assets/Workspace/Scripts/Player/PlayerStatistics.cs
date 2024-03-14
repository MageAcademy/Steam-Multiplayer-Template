using Mirror;

public class PlayerStatistics : NetworkBehaviour
{
    public class Value
    {
        public bool isHighest = false;

        public bool isLowest = false;
    }

    public class FloatValue : Value
    {
        public float value = 0;
    }

    public class IntValue : Value
    {
        public int value = 0;
    }

    public class Data
    {
        public FloatValue dealDamage = new FloatValue();

        public FloatValue dealHealing = new FloatValue();

        public IntValue killCount = new IntValue();

        public string name = null;

        public IntValue rank = new IntValue();
    }

    [SyncVar] public float networkDealDamage = 0f;

    [SyncVar] public float networkDealHealing = 0f;

    [SyncVar] public int networkKillCount = 0;

    [SyncVar] public int networkRank = 0;

    private Player player = null;


    public Data GetData()
    {
        return new Data
        {
            dealDamage = new FloatValue { value = networkDealDamage },
            dealHealing = new FloatValue { value = networkDealHealing },
            killCount = new IntValue { value = networkKillCount },
            name = player.identity.networkSteamName,
            rank = new IntValue { value = networkRank + 1 }
        };
    }


    public void Initialize(Player player)
    {
        this.player = player;
    }
}