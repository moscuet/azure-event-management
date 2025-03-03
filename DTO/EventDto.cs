namespace EventManagementApi.DTO
{
  public class EventCreateDto
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Date { get; set; }
    public string OrganizerId { get; set; }
    public int TotalSpots { get; set; } = 100;
  }
}

public class EventUpdateDto
{
  public string? Name { get; set; }
  public string? Description { get; set; }
  public string? Location { get; set; }
  public string? Date { get; set; }
  public string? OrganizerId { get; set; }
  public int? TotalSpots { get; set; }
}


public class EventReadDto
{
  public Guid Id { get; set; }
  public string Name { get; set; }
  public string Description { get; set; }
  public string Location { get; set; }
  public string Date { get; set; }
  public int TotalSpots { get; set; }
   public int RegisteredCount { get; set; }
  public List<string> ImageUrls { get; set; } = new List<string>();
  public List<string> DocumentUrls { get; set; } = new List<string>();
}
