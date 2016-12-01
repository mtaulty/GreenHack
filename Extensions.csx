using System;

[Serializable]
public class BotUtilities
{
	public BotUtilities()
	{
	}

    public string FormatReply(int count, string message)
    {
        return ($"{count}: I think you said {message}");
    }
}
