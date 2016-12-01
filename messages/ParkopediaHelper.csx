#r "Newtonsoft.Json"

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

  public enum ParkopediaApi
  {
    Search
  }
  public enum SortOrder
  {
    Price,
    Rating,
    Distance,
    Name,
    Availability
  }
  public class ParkopediaApiHelper
  {
    public ParkopediaApiHelper()
    {
      this.deviceId = Guid.NewGuid();

      this.argumentProviders = new Dictionary<string, Func<string>>()
      {
        ["cid"] = GetCid,
        ["fmt"] = GetFmt,
        ["lang"] = GetLang,
        ["dev"] = GetDev,
        ["osver"] = GetOsVer,
        ["u"] = GetU,
        ["apiver"] = GetApiVer,
        ["v"] = GetV        
      };
    }
    public async Task<T> SearchForParkingAsync<T>(string location,
      SortOrder sortOrder = SortOrder.Rating)
    {      
      var parameters = new Dictionary<string, string>()
      {
        [queryParameterName] = location,
        //[sortOrderParameterName] = sortOrder.ToString().ToLower()
      };
      var result = await this.MakeWebRequestAsync<T>(ParkopediaApi.Search, parameters);

      return (result);
    }
    async Task<T> MakeWebRequestAsync<T>(
      ParkopediaApi apiName, 
      Dictionary<string,string> parameters)
    {
      T result = default(T);

      string query = MakeQuery(apiName, parameters);

      var httpClient = new HttpClient();

      var response = await httpClient.GetAsync(new Uri(query, UriKind.Absolute));

      if (response.IsSuccessStatusCode)
      {
        var responseBody = await response.Content.ReadAsStringAsync();

        result = 
          JsonConvert.DeserializeObject<T>(responseBody);
      }
      return (result);
    }
    string MakeQuery(ParkopediaApi apiName, Dictionary<string, string> parameters)
    {
      var relativePath = $"{baseApiUrlPath}/{apiName.ToString().ToLower()}";
      var path = $"{baseServiceUrlFormat}{relativePath}?";

      var queryBuilder = new StringBuilder();

      foreach (var serviceArgument in this.argumentProviders)
      {
        AppendQueryEntry(queryBuilder, serviceArgument.Key, serviceArgument.Value());
      }
      foreach (var parameter in parameters)
      {
        AppendQueryEntry(queryBuilder, parameter.Key, parameter.Value);
      }
      var signature = CalculateSignature(relativePath, parameters);

      AppendQueryEntry(queryBuilder, sigParameterName, signature);

      var query = $"{path}{queryBuilder.ToString()}";

      return(query);
    }

    string CalculateSignature(string path,
      Dictionary<string,string> parameters)
    {
      var serviceArguments = this.argumentProviders.ToDictionary(
        entry => entry.Key,
        entry =>
        {
          var encoded = WebUtility.UrlEncode(entry.Value());

          if (encoded.Length > 32)
          {
            encoded = this.CalculateMd5(encoded);
          }
          return (encoded);
        }
      );
      serviceArguments[pathParameterName] = path;

      foreach (var parameter in parameters)
      {
        serviceArguments.Add(parameter.Key, this.EncodeParameter(parameter.Value));
      }
      var sortedArguments = serviceArguments.OrderBy(entry => entry.Key);

      var builder = new StringBuilder();
      builder.Append(passwordValue);

      foreach (var item in sortedArguments)
      {
        builder.Append(item.Key);
        builder.Append(item.Value);
      }
      var argumentString = builder.ToString();

      var argumentMd5 = this.CalculateMd5(argumentString);

      return (argumentMd5.ToLower());
    }

    static void AppendQueryEntry(StringBuilder queryBuilder, 
      string key, string value)
    {
      var urlEncoded = WebUtility.UrlEncode(value);
      var separator = queryBuilder.Length == 0 ? string.Empty : "&";

      queryBuilder.Append($"{separator}{key}={urlEncoded}");
    }

    string EncodeParameter(string parameter)
    {
      var encoded = WebUtility.UrlEncode(parameter);

      if (encoded.Length > 32)
      {
        encoded = this.CalculateMd5(encoded);
      }
      return (encoded);
    }
    string CalculateMd5(string input)
    {
      if (this.md5 == null)
      {
        this.md5 = MD5.Create();
        this.md5.Initialize();
      }
      var bytes = UnicodeEncoding.UTF8.GetBytes(input);
      var encodedBytes = md5.ComputeHash(bytes);
      return (BitConverter.ToString(encodedBytes).Replace("-", string.Empty));
    }
    string GetCid()
    {
      return (cidValue);
    }
    string GetFmt()
    {
      return (fmtValue);
    }
    string GetLang()
    {
      return (langValue);
    }
    string GetDev()
    {
      return (deviceValue);
    }
    string GetOsVer()
    {
      return (osVerValue);
    }
    string GetU()
    {
      return (this.deviceId.ToString().Replace("-", string.Empty));
    }
    string GetV()
    {
      return (this.vValue);
    }
    string GetApiVer()
    {
      return (apiVerName);
    }
    Dictionary<string, Func<string>> argumentProviders;
    Guid deviceId;
    MD5 md5;

    readonly string baseServiceUrlFormat = "https://api.parkopedia.com";
    readonly string baseApiUrlPath = "/api";
    readonly string cidValue = "microsoft-hackathon_eb796";
    readonly string fmtValue = "json";
    readonly string langValue = "en";
    readonly string deviceValue = "samsung_galaxy";
    readonly string osVerValue = "3.1";
    readonly string queryParameterName = "q";
    readonly string apiVerName = "19";
    readonly string pathParameterName = "_path";
    readonly string passwordValue = "ISFZgSPfiaNoncFN";
    readonly string sigParameterName = "sig";
    readonly string vValue = "1.4";
    readonly string sortOrderParameterName = "sort";
  }
 
