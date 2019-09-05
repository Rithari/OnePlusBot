using System;
namespace OnePlusBot.Data.Models
{
    public class FaqCommandChannelEntryBuilder 
    {
        FAQCommandChannelEntry entry = new FAQCommandChannelEntry();

        public FaqCommandChannelEntryBuilder withText(string value)
        {
            entry.Text = value;
            return this;
        }

        public FaqCommandChannelEntryBuilder withImageUrl(string value)
        {
            entry.ImageURL = value;
            return this;
        }

        public FaqCommandChannelEntryBuilder withAuthor(string value)
        {
            entry.Author = value;
            return this;
        }

        public FaqCommandChannelEntryBuilder withHexColor(uint value)
        {
            entry.HexColor = value;
            return this;
        }

        public FaqCommandChannelEntryBuilder withAuthorAvatarUrl(string value)
        {
            entry.AuthorAvatarUrl = value;
            return this;
        }

        public FaqCommandChannelEntryBuilder withIsEmbed(bool value)
        {
            entry.IsEmbed = value;
            return this;
        }

        public FaqCommandChannelEntryBuilder withChangedDate(DateTime changedDate)
        {
            entry.ChangedDate = changedDate;
            return this;
        }

        public FaqCommandChannelEntryBuilder defaultValues()
        {
            this.withAuthor("r/Oneplus");
            this.withAuthorAvatarUrl("https://cdn.discordapp.com/avatars/426015562595041280/cab7dde68e8da9bcfd61842bd98e950b.png");
            this.withChangedDate(DateTime.Now);
            return this;
        }

        public FAQCommandChannelEntry Build()
        {
            return entry;
        }
    }
}
