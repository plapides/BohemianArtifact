using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BohemianArtifact
{
    public class VagueDate
    {
        public enum Qualifier { Exact, Circa, Between, Before, After };
        public float StartYear;
        public float EndYear;
        public float StartError;
        public float EndError;
        public Qualifier DateQualifier;
        public VagueDate(float startYear, float endYear, float startError, float endError)
        {
            StartYear = startYear;
            EndYear = endYear;
            StartError = startError;
            EndError = endError;
            if (StartError == 0 && EndError == 0 && StartYear == EndYear)
            {
                DateQualifier = Qualifier.Exact;
            }
            else if (StartError == 0 && 0 < EndError)
            {
                DateQualifier = Qualifier.After;
            }
            else if (EndError == 0 && 0 < StartError)
            {
                DateQualifier = Qualifier.Before;
            }
            else if (StartYear != EndYear)
            {
                DateQualifier = Qualifier.Between;
            }
            else if (0 < StartError && 0 < EndError)
            {
                DateQualifier = Qualifier.Circa;
            }
        }
        public VagueDate(float startYear, float endYear, Qualifier qualifier)
        {
            StartYear = startYear;
            EndYear = endYear;
            DateQualifier = qualifier;
            switch (DateQualifier)
            {
                case Qualifier.Exact:
                    StartError = 0;
                    EndError = 0;
                    break;
                case Qualifier.Circa:
                    StartError = 10;
                    EndError = 10;
                    break;
                case Qualifier.Before:
                    StartError = 10;
                    EndError = 0;
                    break;
                case Qualifier.After:
                    StartError = 0;
                    EndError = 10;
                    break;
                case Qualifier.Between:
                    StartError = 0;
                    EndError = 0;
                    break;
            }
        }
        public VagueDate(float startYear, float endYear, float startError, float endError, Qualifier qualifier)
        {
            StartYear = startYear;
            EndYear = endYear;
            StartError = startError;
            EndError = endError;
            DateQualifier = qualifier;
        }

        public static VagueDate.Qualifier ParseQualifier(string qualifier)
        {
            qualifier = qualifier.ToLower();
            if (qualifier == "exact")
            {
                return Qualifier.Exact;
            }
            else if (qualifier == "between")
            {
                return Qualifier.Between;
            }
            else if (qualifier == "circa")
            {
                return Qualifier.Circa;
            }
            else if (qualifier == "before")
            {
                return Qualifier.Before;
            }
            else if (qualifier == "after")
            {
                return Qualifier.After;
            }
            else
            {
                return Qualifier.Exact;
            }
        }
    }
    public class Material
    {
        private string[] primary;
        private string[] secondary;
        public string Primary
        {
            get
            {
                return primary[Artifact.CurrentLanguage];
            }
        }
        public string Secondary
        {
            get
            {
                return secondary[Artifact.CurrentLanguage];
            }
        }
        public Material(string pEnglish, string pFrench, string sEnglish, string sFrench)
        {
            primary = new string[] { pEnglish, pFrench };
            secondary = new string[] { sEnglish, sFrench };
        }
        public override bool Equals(object obj)
        {
            return (Primary == ((Material)obj).Primary);
        }
        public override int GetHashCode()
        {
            return Primary.GetHashCode();
        }
    }
    public class KeywordPair
    {
        private string[] keyword;
        private Artifact artifactRef;
        public string Keyword
        {
            get
            {
                return keyword[Artifact.CurrentLanguage];
            }
        }
        public Artifact ArtifactRef
        {
            get
            {
                return artifactRef;
            }
        }

        public KeywordPair(string KeywordEnglish, string KeywordFrench, Artifact ArtifactRef)
        {
            this.keyword = new string[2] { KeywordEnglish, KeywordFrench };
            this.artifactRef = ArtifactRef;
        }
        public override bool Equals(object obj)
        {
            return (Keyword == ((KeywordPair)obj).Keyword);
        }
        public override int GetHashCode()
        {
            return Keyword.GetHashCode();
        }
    }
    public class StemPair
    {
        private string stem;
        private KeywordPair keywordRef;
        private Artifact artifactRef;
        public string Stem
        {
            get
            {
                return stem;
            }
        }
        public KeywordPair KeywordRef
        {
            get
            {
                return keywordRef;
            }
        }
        public Artifact ArtifactRef
        {
            get
            {
                return artifactRef;
            }
        }

        public StemPair(string Stem, KeywordPair KeywordRef, Artifact ArtifactRef)
        {
            this.stem = Stem;
            this.keywordRef = KeywordRef;
            this.artifactRef = ArtifactRef;
        }
        public override bool Equals(object obj)
        {
            return (stem == ((StemPair)obj).Stem);
        }
        public override int GetHashCode()
        {
            return stem.GetHashCode();
        }
    }

    public class Artifact
    {
        private const string LANGUAGE_SEPARATOR = ";:;";

        public const int LANGUAGE_ENGLISH = 0;
        public const int LANGUAGE_FRENCH = 1;
        public static int CurrentLanguage = LANGUAGE_ENGLISH;
        public static List<string> Stopwords;

        private string mCatalogNumber;
        private string[] mArticleName;
        private List<Material> mMaterials;
        private List<KeywordPair> mKeywords;
        private List<StemPair> mStems;

        private VagueDate catalogDate;
        private VagueDate manufactureDate;
        private VagueDate useDate;

        private Texture2D texture;
        private Color color;
        private SelectableText[] text;

        public VagueDate CatalogDate
        {
            get
            {
                return catalogDate;
            }
        }
        public VagueDate ManufactureDate
        {
            get
            {
                return manufactureDate;
            }
        }
        public VagueDate UseDate
        {
            get
            {
                return useDate;
            }
        }
        public string CatalogNumber
        {
            get
            {
                return mCatalogNumber;
            }
        }
        public string ArticleName
        {
            get
            {
                return mArticleName[CurrentLanguage];
            }
        }
        public List<Material> Materials
        {
            get
            {
                return mMaterials;
            }
        }
        public List<KeywordPair> Keywords
        {
            get
            {
                return mKeywords;
            }
        }
        public List<StemPair> Stems
        {
            get
            {
                return mStems;
            }
        }
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
        }
        public Color Color
        {
            get
            {
                return color;
            }
        }
        public SelectableText Text
        {
            get
            {
                return text[CurrentLanguage];
            }
        }

        public static void LoadStopwords()
        {
            Stopwords = new List<string>();
            StreamReader stopwordFile = new StreamReader("stopwords.txt");
            string line;
            while ((line = stopwordFile.ReadLine()) != null)
            {
                Stopwords.Add(line);
            }
            stopwordFile.Close();
        }

        public Artifact(XmlNode node)
        {
            // initialize for english/french language
            mArticleName = new string[2];

            mMaterials = new List<Material>();
            mKeywords = new List<KeywordPair>();
            mStems = new List<StemPair>();

            text = new SelectableText[2];

            // start pulling data from XML into this artifact

            // catalog number (non-language specific)
            mCatalogNumber = node.ChildNodes[0].InnerText;
            texture = XNA.LoadTexture(BohemianArtifact.TexturePath + "artifacts\\" + mCatalogNumber + ".jpg");

            // the date it was added to the catalog
            float catalogYear = Convert.ToSingle(mCatalogNumber.Substring(0, mCatalogNumber.IndexOf(".")));
            catalogDate = new VagueDate(catalogYear, catalogYear, 0, 0);

            // manufacture date
            float yearStart = 0;
            float yearEnd = 0;
            string qualifier = "";

            // get manufacture date
            try
            {
                yearStart = Convert.ToSingle(node.ChildNodes[6].InnerText);
                if (node.ChildNodes[7].InnerText != "")
                {
                    yearEnd = Convert.ToSingle(node.ChildNodes[7].InnerText);
                }
                else
                {
                    yearEnd = yearStart;
                }
                qualifier = ExtractLanguage(LANGUAGE_ENGLISH, node.ChildNodes[5].InnerText);
            }
            catch (Exception e) { }
            manufactureDate = new VagueDate(yearStart, yearEnd, VagueDate.ParseQualifier(qualifier));

            // get use date
            yearStart = 0;
            yearEnd = 0;
            qualifier = "";
            try
            {
                yearStart = Convert.ToSingle(node.ChildNodes[26].InnerText);
                if (node.ChildNodes[27].InnerText != "" && node.ChildNodes[27].InnerText != " ")
                {
                    yearEnd = Convert.ToSingle(node.ChildNodes[27].InnerText);
                }
                else
                {
                    yearEnd = yearStart;
                }
                qualifier = ExtractLanguage(LANGUAGE_ENGLISH, node.ChildNodes[25].InnerText);
            }
            catch (Exception e) { }
            useDate = new VagueDate(yearStart, yearEnd, VagueDate.ParseQualifier(qualifier));
            
            // article name
            mArticleName[LANGUAGE_ENGLISH] = ExtractLanguage(LANGUAGE_ENGLISH, node.ChildNodes[1].InnerText);
            mArticleName[LANGUAGE_FRENCH] = ExtractLanguage(LANGUAGE_FRENCH, node.ChildNodes[1].InnerText);

            text[LANGUAGE_ENGLISH] = new SelectableText(XNA.Font, mArticleName[LANGUAGE_ENGLISH]);
            text[LANGUAGE_FRENCH] = new SelectableText(XNA.Font, mArticleName[LANGUAGE_FRENCH]);

            string col = node.ChildNodes[28].InnerText;
            int r = Convert.ToInt32(col.Substring(1, 3));
            int g = Convert.ToInt32(col.Substring(5, 3));
            int b = Convert.ToInt32(col.Substring(9, 3));
            color = new Color(r, g, b);
            //r12rg456b789
            //012345678901

            // materials
            if (node.ChildNodes[23] == null)
            {
                // no secondary materials
                PopulateMaterials(node.ChildNodes[22].InnerText, "");
            }
            else
            {
                // secondary materials exist
                PopulateMaterials(node.ChildNodes[22].InnerText, node.ChildNodes[23].InnerText);
            }

            // keywords
            AddKeywordsStems(node.ChildNodes[8].InnerText);
            AddKeywordsStems(node.ChildNodes[9].InnerText);
            AddKeywordsStems(node.ChildNodes[10].InnerText);
        }

        private string ExtractLanguage(int language, string input)
        {
            if (input.Length == 0 || input.IndexOf(LANGUAGE_SEPARATOR) == -1)
            {
                // if either input is "" or doesn't have a separator (english only)
                return input;
            }
            else
            {
                return input.Split(new string[1] { LANGUAGE_SEPARATOR }, StringSplitOptions.None)[language];
            }
        }

        private string StripPossible(string input)
        {
            if (input.IndexOf(" - possible") == -1)
            {
                return input;
            }
            else
            {
                return input.Substring(0, input.IndexOf(" - possible"));
            }
        }

        private void PopulateMaterials(string primaryText, string secondaryText)
        {
            string[] primary = primaryText.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] secondary = secondaryText.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < primary.Length; i++)
            {
                if (primary[i] == "")
                {
                    continue;
                }

                string pEnglish = ExtractLanguage(LANGUAGE_ENGLISH, primary[i]);
                string pFrench = ExtractLanguage(LANGUAGE_FRENCH, primary[i]);
                string sEnglish, sFrench;
                if (i < secondary.Length)
                {
                    sEnglish = ExtractLanguage(LANGUAGE_ENGLISH, secondary[i]);
                    sFrench = ExtractLanguage(LANGUAGE_FRENCH, secondary[i]);
                }
                else
                {
                    sEnglish = "";
                    sFrench = "";
                }

                // strip "possible"
                pEnglish = StripPossible(pEnglish);
                pFrench = StripPossible(pFrench);
                sEnglish = StripPossible(sEnglish);
                sFrench = StripPossible(sFrench);

                Material material = new Material(pEnglish, pFrench, sEnglish, sFrench);
                mMaterials.Add(material);
            }
        }

        private void AddKeywordsStems(string unparsed)
        {
            string[] chunks = unparsed.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string chunk in chunks)
            {
                // add english keywords to the list first
                string longKeywordEnglish = ExtractLanguage(LANGUAGE_ENGLISH, chunk);
                string longKeywordFrench = ExtractLanguage(LANGUAGE_FRENCH, chunk);
                KeywordPair keywordPair = new KeywordPair(longKeywordEnglish, longKeywordFrench, this);
                if (longKeywordEnglish != "" && longKeywordFrench != "")
                {
                    mKeywords.Add(keywordPair);
                }

                // now get the stems for each keyword, using the english keywords since that is what the stemmer works for
                // split each english keyword into separate words
                string[] preStemWordList = longKeywordEnglish.Split(new string[4] { " ", "-", "&", "," }, StringSplitOptions.None);
                foreach (string word in preStemWordList)
                {
                    // and get the stem for each word
                    string stem = Stemmer.Stem(word.ToLower());
                    StemPair stemPair = new StemPair(stem, keywordPair, this);
                    if (stem != "" && Stopwords.Contains(stem) == false)
                    {
                        mStems.Add(stemPair);
                    }
                }
            }
        }
    }

    public class ArtifactLibrary
    {
        public delegate void SelectedArtifactHandler(Artifact selectedArtifact);
        public event SelectedArtifactHandler SelectedArtifactChanged;

        private Artifact selectedArtifact;
        private List<Artifact> artifacts;
        public Artifact SelectedArtifact
        {
            get
            {
                return selectedArtifact;
            }
            set
            {
                SelectArtifact(value);
            }
        }
        public List<Artifact> Artifacts
        {
            get
            {
                return artifacts;
            }
        }

        private Dictionary<string, List<StemPair>> stemGraph;
        public Dictionary<string, List<StemPair>> StemGraph
        {
            get
            {
                return stemGraph;
            }
        }

        public ArtifactLibrary(string filename)
        {
            artifacts = new List<Artifact>();

            Artifact.LoadStopwords();

            // load the xml doc
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNode worksheet = doc.DocumentElement.ChildNodes[4];
            XmlNode table = worksheet.FirstChild;
            bool firstRow = false;
            int numArtifactsLoaded = 0;
            foreach (XmlNode child in table.ChildNodes)
            {
                // this check is to load only those artifacts whose 512px images exist
                //if (66 < numArtifactsLoaded)
                if (52 < numArtifactsLoaded)
                {
                    //break;
                }

                if (child.Name == "Row")
                {
                    if (firstRow == false)
                    {
                        // skip the first row
                        firstRow = true;
                        continue; 
                    }
                    if (child.ChildNodes.Count == 29)
                    {
                        Artifact newArtifact = new Artifact(child);
                        numArtifactsLoaded++;
                        if (0 < newArtifact.Materials.Count)
                        {
                            artifacts.Add(newArtifact);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not loaded! ChildNodes.Count = " + child.ChildNodes.Count + " -- " + child.ChildNodes[1].InnerText);
                    }
                }
            }

            /*
            //Artifact.CurrentLanguage = Artifact.LANGUAGE_FRENCH;
            FileStream frenchFile = new FileStream("french_spelling.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(frenchFile);
            foreach (Artifact a in artifacts)
            {
                sw.WriteLine(a.ArticleName);
            }
            sw.Flush();
            frenchFile.Close();
            //*/

            // compute a graph of all artifact keywords/stems
            stemGraph = new Dictionary<string, List<StemPair>>();
            ComputeStemGraph();

            /*
            foreach (Artifact artifact in artifacts)
            {
                int relatedArtifacts = 0;
                foreach (StemPair sp in artifact.Stems)
                {
                    List<StemPair> relatedStems = stemGraph[sp.Stem];
                    relatedArtifacts += relatedStems.Count;
                }
                if (relatedArtifacts < 2)
                {
                    Console.WriteLine(artifact.ArticleName + " has no related artifacts.");
                }
            }

            foreach (string stem in stemGraph.Keys)
            {
                // don't bother printing out the stem graph
                continue;
                List<StemPair> stemList = stemGraph[stem];
                if (100 < stemList.Count)
                {
                    Console.WriteLine(stem + " => ");
                    foreach (StemPair pair in stemList)
                    {
                        Console.WriteLine("\t" + pair.ArtifactRef.ArticleName);
                    }
                }
            }
            //*/

            Dictionary<string, int> materialsTally = GetMaterialsTally(new List<string>());
            foreach (string s in materialsTally.Keys)
            {
                //Console.WriteLine(s + ": " + materialsTally[s]);
            }
            //for (int i = 0; i < 10; i++)
            //{
            //    foreach (StemPair stemPair in artifacts[i].Stems)
            //    {
            //        Console.WriteLine(stemPair.Stem + " => " + stemPair.KeywordRef.Keyword);
            //    }
            //    Console.WriteLine("---");
            //}
        }

        private void ComputeStemGraph()
        {
            foreach (Artifact artifact in artifacts)
            {
                foreach (StemPair stemPair in artifact.Stems)
                {
                    string stem = stemPair.Stem;
                    if (stemGraph.ContainsKey(stem) == false)
                    {
                        // graph doesn't have this stem yet
                        List<StemPair> newStemList = new List<StemPair>();
                        newStemList.Add(stemPair);
                        stemGraph.Add(stem, newStemList);
                    }
                    else
                    {
                        // stem exists in the graph
                        List<StemPair> stemList = stemGraph[stem];
                        if (stemList == null)
                        {
                            Console.WriteLine("Error: stem list for '" + stem + "' in stemGraph is missing.");
                            continue;
                        }

                        stemList.Add(stemPair);
                    }
                }
            }
        }

        private List<string> GetKeywordStems()
        {
            List<string> stems = new List<string>();
            foreach (Artifact a in artifacts)
            {
                foreach (StemPair stemPair in a.Stems)
                {
                    if (stems.Contains(stemPair.Stem) == false)
                    {
                        stems.Add(stemPair.Stem);
                    }
                }
            }
            return stems;
        }

        public Dictionary<string, int> GetMaterialsTally(List<string> materialConstraints)
        {
            Dictionary<string, int> tally = new Dictionary<string, int>();

            foreach (Artifact a in artifacts)
            {
                // for each artifact, compare it's material list to the materialConstraints list
                bool completeMatch = true;
                foreach (string s in materialConstraints)
                {
                    if (s == "")
                    {
                        continue;
                    }
                    // check if this artifact has the constraint material
                    bool partialMatch = false;
                    foreach (Material m in a.Materials)
                    {
                        if (m.Primary == s)
                        {
                            // there is a match, no need to keep checking
                            partialMatch = true;
                            break;
                        }
                    }
                    // if there was NOT a partial match, then this artifact does NOT have all of the materials in the constraints list
                    if (partialMatch == false)
                    {
                        completeMatch = false;
                        break;
                    }
                }

                if (completeMatch == true)
                {
                    // if there was a complete match, then add this artifact's materials to the tally
                    foreach (Material m in a.Materials)
                    {
                        if (tally.ContainsKey(m.Primary) == false)
                        {
                            tally.Add(m.Primary, 1);
                        }
                        else
                        {
                            tally[m.Primary] = (int)tally[m.Primary] + 1;
                        }
                    }
                }
            }

            foreach (string s in materialConstraints)
            {
                tally.Remove(s);
            }

            return tally;
        }

        public void SelectArtifact(Artifact sArtifact)
        {
            if (artifacts.Contains(sArtifact) == true)
            {
                selectedArtifact = sArtifact;
                if (SelectedArtifactChanged != null)
                {
                    SelectedArtifactChanged(selectedArtifact);
                }
            }
        }
    }
}
