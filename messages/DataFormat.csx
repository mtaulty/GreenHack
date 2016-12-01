  public class ServiceResponse
  {
    public string status { get; set; }
    public Result result { get; set; }
    public string error { get; set; }
    public string errorcode { get; set; }

    public bool IsNoParking
    {
        get
        {
            return (this.status == "ERROR" && this.errorcode == "ERR_SEARCH_NOTFOUND");
        }
    }
    public bool IsValid
    {
      get
      {
        return (this.status == "OK");
      }
    }
  }

  public class Result
  {
    public string q { get; set; }
    public string maplat { get; set; }
    public string maplng { get; set; }
    public string mapzoom { get; set; }
    public Space[] spaces { get; set; }
    public Zones zones { get; set; }
    public Filter[] filters { get; set; }
  }

  public class Zones
  {
    public object[] legend { get; set; }
    public object[] polygons { get; set; }
  }

  public class Space
  {
    public string id { get; set; }
    public string type { get; set; }
    public string url { get; set; }
    public bool pr { get; set; }
    public Icon icon { get; set; }
    public Rating rating { get; set; }
    public string city { get; set; }
    public string[] addresses { get; set; }
    public Dgroup[] dgroups { get; set; }
    public Image[] images { get; set; }
    public string phone { get; set; }
    public string dial { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
    public string info { get; set; }
    public string col1 { get; set; }
    public string col2 { get; set; }
    public string col3 { get; set; }
    public string filters { get; set; }
    public string typeid { get; set; }
    public string timezoneid { get; set; }
    public Streetview streetview { get; set; }
    public Geometry[] geometry { get; set; }
    public Sort sort { get; set; }

	public override ToString()
	{
		return($"Parking of Type {this.type} at {this.col1}, {this.col2}, {this.col3}");
	}
  }

  public class Icon
  {
    public string tag { get; set; }
  }

  public class Rating
  {
    public string score { get; set; }
    public int numreviews { get; set; }
    public int numvotes { get; set; }
  }

  public class Streetview
  {
    public string lat { get; set; }
    public string lng { get; set; }
    public string yaw { get; set; }
    public string pitch { get; set; }
    public string zoom { get; set; }
  }

  public class Sort
  {
    public int distance { get; set; }
    public int price { get; set; }
    public int name { get; set; }
    public int rating { get; set; }
  }

  public class Dgroup
  {
    public string name { get; set; }
    public string type { get; set; }
    public Item[] items { get; set; }
    public int idx { get; set; }
    public string value { get; set; }
  }

  public class Item
  {
    public string val { get; set; }
    public int bold { get; set; }
    public int subscript { get; set; }
  }

  public class Image
  {
    public string url { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
    public string heading { get; set; }
    public string typeid { get; set; }
    public string photoid { get; set; }
    public string source { get; set; }
    public string phototype { get; set; }
  }

  public class Geometry
  {
    public string type { get; set; }
    public string value { get; set; }
  }

  public class Filter
  {
    public bool x { get; set; }
    public string label { get; set; }
    public string type { get; set; }
    public int code { get; set; }
    public int state { get; set; }
  }
