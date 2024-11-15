namespace MonsterTradingCardsGameLibrary
{
    public abstract class Card
    {
        public int Id { get; protected set; }

        public string Name { get; protected set; }

        public int Damage { get; protected set; }

        public ElementType ElementType { get; protected set; }

        public Card(int id, string name, int damage, ElementType elementType)
        {
            Id = id;
            Name = name;
            Damage = damage;
            ElementType = elementType;
        }
    }
}
