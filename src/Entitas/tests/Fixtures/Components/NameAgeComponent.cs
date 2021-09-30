public class NameAgeComponent : Entitas.IComponent
{
    public string Name;
    public int Age;

    public override string ToString()
    {
        return $"NameAge({Name}, {Age})";
    }
}
