using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

var client = new HttpClient() { BaseAddress = new Uri("https://api.jsonbin.io/b/5fb62f9404be4f05c9272f9b/2") };
client.DefaultRequestHeaders.Add("secret-key", "$2b$10$FRKBdPsyV27ReVeMpo83U.BYS0Jko5KUjctlj0A865qRkEYVf49ty");

var dataService = new DataService(client);
var items = await dataService.GetItemsAsync();

var nodes = items.ToTree();
var display = nodes.MakeFormattedString();

Console.WriteLine(display);

public static class Node2Tree
{	
	// Convert flat id-parent list to tree (METHOD 1)	
	public static Node ToTree(this IEnumerable<Item> items)
	{
		if (items == null) return new Node();
		var rootItem = items.FirstOrDefault(f => f.Parent == -1);
		if (rootItem == null) throw new Exception("Have no any parent nodes");
		var rootNode = ConvertToNode(rootItem);
		GenerateTree(rootNode, items);
		return rootNode;
	}

	// Convert flat id-parent list to tree (METHOD 2 - another syntax with yield)
	////public static Node ToTree(this IEnumerable<Item> items)
	////{
	////	if (items == null) return new Node();

	////	return items.GenerateTree().FirstOrDefault();
	////}	

	////private static IEnumerable<Node> GenerateTree(this IEnumerable<Item> items, int parentId = -1)
	////{
	////	var childItems = items.Where(c => c.Parent == parentId);

	////	foreach (var c in childItems)
	////	{
	////		yield return new Node
	////		{
	////			Id = c.Id,
	////			Text = c.Text,
	////			Childs = items.GenerateTree(c.Id).ToList()
	////		};
	////	}
	////}

	// Make string with nmarkup hierarchy of nodes
	public static string MakeFormattedString(this Node node)
	{
		var result = new StringBuilder();
		PrintTree(node, result, 0);
		return $"<ul>\n{result}</ul>";
	}	

    private static void GenerateTree(Node node, IEnumerable<Item> items)
    {
        var childItems = items.Where(item => item.Parent == node.Id).ToArray();
        foreach (var childItem in childItems)
        {
            node.Childs.Add(ConvertToNode(childItem));
        }

        foreach (var nodeItem in node.Childs)
        {
            GenerateTree(nodeItem, items);
        }
    }

    private static Node ConvertToNode(Item item)
	{
		return new Node
		{
			Id = item.Id,
			Text = item.Text,
			Childs = new List<Node>()
		};
	}

	private static void PrintTree(Node node, StringBuilder result, int level)
	{
		if (node == null) return;
		var spaces = new string(' ', (level + 1) * 2);
		result.AppendLine($"{spaces}<li>{node.Id}. {node.Text} ({level})</li>");
		if (!node.HasChilds) return;
		result.AppendLine($"{spaces}<ul>");
		level++;

		foreach (var item in node.Childs)
		{
			PrintTree(item, result, level);
		}

		result.AppendLine($"{spaces}</ul>");
	}
}

public class Node
{
	public int Id { get; set; }
	public string Text { get; set; }
	public List<Node> Childs { get; set; }

	public bool IsRoot => Id == 0;
	public bool HasChilds => Childs != null && Childs.Any();
}

public class DataService
{
	private readonly HttpClient _httpClient;
	public DataService(HttpClient httpClient) => _httpClient = httpClient;

	public async Task<IEnumerable<Item>> GetItemsAsync()
	{
		using var resp = await _httpClient.GetAsync("");
		var json = await resp.Content.ReadAsStringAsync();

		Console.WriteLine(json);
		return JsonConvert.DeserializeObject<Item[]>(json);
	}
}

public record Item
{
	public int Id { get; set; }
	public string Text { get; set; }
	public int Parent { get; set; }
}


