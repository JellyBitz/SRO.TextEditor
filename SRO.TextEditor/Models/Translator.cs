using GTranslate.Translators;

namespace SRO.TextEditor.Models
{
    /// <summary>
    /// Translator that loads his engine when it's required based on his name.
    /// </summary>
    public class Translator
    {
        private ITranslator mTranslator;
        public string Name;
        public ITranslator GetInstance()
        {
            if (mTranslator == null)
            {
                switch (Name)
                {
                    case nameof(GoogleTranslator2):
                        mTranslator = new GoogleTranslator2();
                        break;
                    case nameof(BingTranslator):
                        mTranslator = new BingTranslator();
                        break;
                    case nameof(MicrosoftTranslator):
                        mTranslator = new MicrosoftTranslator();
                        break;
                    case nameof(YandexTranslator):
                        mTranslator = new YandexTranslator();
                        break;
                    case nameof(GoogleTranslator):
                        mTranslator = new GoogleTranslator();
                        break;
                }
            }
            return mTranslator;
        }
    }
}
