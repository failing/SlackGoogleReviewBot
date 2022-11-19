using SlackNet.Blocks;

public static class SlackUiBlockHelp
{
    public static string GetStartCount(double rating)
    {
        int roundedRating = Math.Max(1, (int)Math.Round(rating, 1));

        return string.Concat(Enumerable.Repeat("⭐", roundedRating));
    }

    public static HeaderBlock GetHeaderBlock(string name) => new HeaderBlock() { Text = new PlainText() { Emoji = true, Text = name } };
    public static SectionBlock GetDescriptionBlock(string name, double rating) => new SectionBlock() { Text = new PlainText() { Emoji = true, Text = $"{name} ------ {SlackUiBlockHelp.GetStartCount(rating)}" } };
    public static ImageBlock GetImageBlock(string altText, string imageUrl) => new ImageBlock() { AltText = altText, ImageUrl = imageUrl };
}