using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace EcommFashion.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : EcommFashion.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Nivax Data");

            var group1 = new SampleDataGroup("Group-1",
                    "Front Now",
                    "Group Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "10 Crosby Derek Lam",
                    "",
                    "Assets/HubPage/HubPage1.png",
                    "At 10 Crosby Derek Lam, the bold colors and punchy prints that have been mainstays the past several seasons were dialed down in favor of a new emphasis on texture and textiles. Design director Elizabeth Giardina said she and Lam hoped to cultivate the 10 C customer's natural yet smart approach to style with a tactile lineup full of hushed nude tones and rich neutrals. A classically tailored overcoat cut from glossy faux pony hair set a strong tone for outerwear, while a full selection of weekend knits ranged from clingy, delicate cardigans to chunky peplum sweaters. Giardina chose somewhat scratchy fabrics (they weren't half as bad as that sounds), like the nubby wool of a cummerbund-detail jacket or a long-sleeve party sheath made from allover holographic sequins with a scored, imperfect effect. Accessories are an expanding category, and the pointy, zippered patent flats and to-the-knee python boots energized a collection that was, all things considered, the contemporary line's most sophisticated to date.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                   "Céline",
                    "",
                    "Assets/HubPage/HubPage2.png",
                    "A runway made of chipboard. Fuzzy plinths to sit on. The same texture on the invitation. A trend book awaiting each guest with images of clouds, yarn, and fluff, Flemish painting from the fifteenth century, portraits of women and men, the buttocks of statues, mostly female. These were the clues for Phoebe Philo's latest show. What on Earth has the Céline woman, the character Philo has created for her collections, been up to since last we left her? You can't resist imagining the life of the Céline woman; it is quite an extraordinary one. She has some wardrobe for a start—one so precise she wouldn't sit down in it until last season. Of course, she has issues—who among us doesn't?—and she seemed to be working through many of them for Spring. In so doing she became a far more human creation. Now, having gone through her somewhat louche period, it appears as if the Céline woman may be settling down.\n\nFor one, she isn't wearing furry slippers anymore. Yet she hasn't entirely abandoned the idea of domesticity and warmth. If anything, Philo has increased the quotient this season in her collection. Yet the designer has also melded a mood of stripped-down, put-together elegance, something of the old Céline woman combined with the new.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Fendi",
                    "",
                    "Assets/HubPage/HubPage3.png",
                    "FF insignia was originally a sixties thing that stood for Fun Furs. Somehow it became codified as the corporate logo. Today, with his 96th (!!!) collection for the house, Karl Lagerfeld restored that original inflection. Which was simply one more testament to his unerring ability to mount, master, and extrapolate the merest flicker of modernity.Admittedly, he has a brilliant consort in Silvia Venturini Fendi, a woman whose iconoclastic attitude to the verities of her family business inclines her to the extreme. So, with the fashion world surrendering this season to fur in every possible way (it's as though it could resist for only so long, just as compassion fatigue sets in in the world of charitable works), it was the moment for Fendi to boldly go where no man could go before—or after. Or even in between. Simply because, as Lagerfeld so crisply declared, nobody does it better.\n\nThe fun was in the function. The Fendi show rolled by in a farrago of fur, detailed by Lagerfeld in a press-kit illustration that had fur on bags, bangles, belts, booties, and sunglasses. Sam McKnight attached Mohawks of colored fox to the models' heads. Silvia hung fur owls off her bags. Strange creatures on the catwalk, she mused. Like little birds. By the time Julia Nobis closed the show in a fur coat woven in shades of pink and black and framed in barbaric shag, Silvia's little birds had evolved into a new subspecies in fashion: an elegant, extravagant, techno-barbarian riposte to the realities of everyday dress.\n\nIt wasn't just pelts. Karl conspiratorially communicated the properties of something called leather feather fur. We took that to mean the skins fringed and/or trimmed to look like an exercise in pure texture. Because the collection's attention to leather was just as persuasive as its focus on fur. The notion of the natural mutated into something entirely artificial seemed to embody the essence of FF Fendi. Hybrids—they are the strange creatures that Fendi has made. And today they ruled.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Givenchy",
                    "",
                    "Assets/HubPage/HubPage4.png",
                    "Just beautiful. Riccardo Tisci's Givenchy show tonight was one of those fashion moments that true believers slog through four weeks of shows for. It gave you goose bumps. Antony Hegarty of Antony and the Johnsons, a longtime friend of the designer's, performed three songs, establishing a mood that was heartfelt and tender. He opened with You Are My Sister. Tisci, of course, is the youngest of nine children, all the others girls. This collection was the Givenchy frontman at his most personal and romantic, riffing on pieces from his eight-year history at the house, the faint whiff of nostalgia balanced by its fierce nowness. \n\nSwarmed by friends and fans backstage, he said, I always go to the Givenchy archives. By accident I was in the room with all my stuff, and I found things I did when I was younger that I did here in different ways. It's eight years this season that I've been at the house. I was like a gypsy—you know, gypsies are always recycling old clothes. It was really one of the most fun collections I've done in my career.\n\nFun for the audience, too, who checked off the references as they came strutting by on striped snakeskin boots. No one is more responsible for fashion's current fixation on the sweatshirt than Tisci; acknowledging the fact, he opened with a new one, its front emblazoned with Bambi, more Disney-cute than his previous prints. A grunge element came through in plaids and leathers, and oversize sweaters got a fair share of his attention, too; one was paired with a sheer tulle ankle-length skirt embroidered with purple and yellow flowers that called to mind the designer's panthers and lilies collection. \n\nAs boyish as the sweatshirt is, one of Tisci's big ideas this season put the accent on the feminine. A significant number of the looks were cinched at the waist with Perfectos whose tops had been shorn off—glorified belts, really, that created a provocative, peplumed silhouette. And let us not forget the flowers and paisleys, which bloomed and swirled on butch jackets and sheer femme skirts, in lush contrast to the monastic whites and blues of Spring. The models wore matching bracelets from which dangled big, engraved medals. A fitting accessory for what could very well go down as the show of the season.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Haider Ackermann",
                    "",
                    "Assets/HubPage/HubPage5.png",
                    "Strength and fragility are Haider Ackermann's lasting preoccupations, the subjects he returns to each season when you see him backstage. It's a very thin line between them, he said today. Still, this controlled collection came down on the side of strength more so than some previous seasons have. Ackermann reduced his dress offerings to just one for Fall. From a designer famous for straps spilling off shoulders, twisting seams, and trailing hems, the smoky gray panne velvet column gown here was notable for its spare simplicity. Tailoring is where his interests lie at this moment. Some of it was awkwardly oversize, with full-legged pants pooling around the model's ankles, or droopy sleeves extending almost to the knees. Those pieces were moody and evocative as all get out—like a girl in her soldier lover's uniform, or, as the designer suggested afterward, like Marilyn Monroe emerging from the hospital all bundled up against the paparazzi. (Monroe was on the sound track.) But they were also a bit indulgent on Ackermann's part. \n\nWhen you see his clothes in the front rows—and we've seen plenty this week, not just on Tilda Swinton—it's the sharper pieces that his customers gravitate to, with strong shoulders and defined waists. Gratifyingly, there was more of that sort of thing on the runway in the form of a houndstooth military jacket with the collar torn off and the seams left raw, or another jacket in surplus brown with purple velvet lining its ruffled peplum. \n\nFur is new territory for Ackermann, but he proved a dab hand. Women will surely put the power of their wallets behind his shearling flight jackets and his collarless beaver-fur coats. Those were the kind of indulgences that customers will find hard to resist.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                    "Marc Jacobs",
                    "",
                    "Assets/HubPage/HubPage6.png",
                    "A giant sun was suspended from the ceiling of the Lexington Avenue Armory for tonight's Marc Jacobs show, postponed from his usual Monday night slot due to delivery issues. The orb turned your seatmates' faces a startling shade of yellow, but when you looked across the enormous concrete stage, everything was in shades of gray, almost like an old sepia photo. It had the same desaturating effect on the clothes Jacobs sent out. You could make out patterns, like the microplaid of a simple shirtdress, and you could see texture, such as the mohair of snug tops, the fox fur on coats, or the flash of sequins, but you couldn't really determine their color beyond guessing whether something was light or dark. If he had left it at that, it would have been an intriguing though ultimately frustrating experience. But then Jacobs turned up the house lights and sent all 55 models, who wore matching shag wigs, out again to repeat the circuit. It was then you noticed that some of the plaid actually sparkled and that from dress to dress the sequins changed from navy to burgundy to rose to shimmering gold.\n\nThis isn't the first time that Jacobs has fiddled with the traditional runway show format—several years ago, he staged a show back to front. But why send the clothes out twice, first colorless, then not? Jacobs lifted the low-frequency light idea from Olafur Eliasson's The Weather Project at the Tate in London, a show that resonated with him after his newly rebuilt West Village house was badly damaged by Hurricane Sandy. Last season was all black and white, and life unfortunately isn't that way, it's all the shades of gray, he said backstage. I've felt out of sorts, and I wanted to see things sort of dismal and then still show the optimistic side.Somewhere Over the Rainbow, a song we've heard at an MJ show before, was the other obvious reference point.\n\nAs for the clothes themselves, they were stripped down and irony-free: cable-knit sweaters, tailored blazers and vests, silk pajamas, fox chubbies, scads of high-waisted briefs—all familiar from Jacobs' oeuvre. The fact that the designer came out for his bow in pajamas of his own (Prada, for the record) offered a clue. He was after the comfort of the familiar. In a New York season strong on real-life clothes, the straightforwardness of that approach resonated. Partially led by Jacobs himself, fashion has been dominated these last few years by high-concept and often overelaborate clothes, and tonight's new direction felt right. There were terrific coats here for days, as well as neat little office-bound sweater and pencil skirt sets. For after-dark, Jacobs layered on those sequins: the most striking looks a pair of evening coats in oversize paillettes with plush fox fur draped around the neck. When the models came out for the finale, they assembled themselves into an orb of their own. Only Marc could turn a bout of melancholy and such simple clothes into the show of the week.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);

             var group2 = new SampleDataGroup("Group-2",
                    "Ready-to-wear",
                    "Group Subtitle: 2",
                    "Assets/LightGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
					"Akris",
					"",
                    "Assets/HubPage/HubPage7.png",
                    "Albert Kriemler lost his mother, Ute, last December. His show today, accompanied by a small orchestra playing her favorite composer, Bach, was a tribute to her, he explained backstage. Although it was almost all black, and inspired by her personal wardrobe of turtleneck gowns, blouse-and-pant combos, and clean tailoring, his new Akris collection wasn't necessarily somber.\n\nThough in demeanor still mournful, Kriemler found interesting ways of letting in the light. Starting with the house's signature photoprints: This season, he used a dark photograph of a street, the streetlamps casting horizontal white lines across the planes of what he called his new three-piece suit—a double-face cocoon coat worn over a double-breasted jacket and a tunic dress. A nubby, three-dimensional St. Gallen embroidery added shimmer to a short cape and its matching pencil skirt, while a floor-grazing skirt in unlined lamb's fur had its own sheen. Other embellishments, including gridlike patterns of jet crystals and silk fringe, lit up his evening offerings. The fringe was contained by horizontal seams; Kriemler didn't seem quite up to the cheerfulness that loose fringe might've suggested.\n\nAnd yet there were other intimations of a son and a company moving on. Kriemler had some brilliant sportswear on the runway today; his customer will love the look of an oversize herringbone sweater and snood worn with matching chevroned pants, or a pantsuit in a tonal plaid cut from cashmere, angora, and wool. In right about the middle of the show, he included a single white lamb's-fur coat. That bright moment served to underscore the poignancy of this season's story, and one couldn't help but be moved.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Brood",
                    "",
                    "Assets/HubPage/HubPage8.png",
                    "Want to make a lovely dress? That's easy—buy a pattern. Want to make a dress that's interesting, or even (aiming higher now) engrossing? For that, you'll have to find new ideas, something that Brood's Serkan Sarier has proven he possesses during his six seasons on the scene. Brood is always about pairing opposites, Sarier said at his Fall 2013 presentation. The idea is merging two things that seem completely opposite into something that hopefully is harmonious. The opposites attracting fashionable eyes today were an eighteenth-century Provençal floral print and monochromatic circles culled from the work of op-art pioneer Victor Vasarely. \n\nIt was as if Damien Hirst sent his assistants to work on one of William Morris' proper wallpaper florals, simultaneously debasing and energizing the traditional motif.Sarier's hybrid print lent itself equally well to a stiff faille skirt with trapunto stitching at the hem as it did to a long, cowl-necked silk georgette dress. The most interesting pieces made use of couturelike techniques to create ballooning volumes. One look that resembled a corseted bodice and full, New Look skirt turned out to be a one-piece dress. Zippers, toggles, and drawstrings balanced the more formal elements and introduced a sharp sportiness throughout. A gray wool skirt, with inverted pleats in the print, and the coordinating tailored, print-lined hoodie looked like strong prospects to see mixed into plugged-in wardrobes soon.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                     "Custo Barcelona",
                    "",
                    "Assets/HubPage/HubPage9.png",
                    "It's not for everybody, designer Custo Dalmau said the Saturday before his Fall show, going through looks with the team at his Soho boutique. Indeed it is not. An utter mishmash of fabrics, colors, materials, and prints, it's easy to knock a collection that many would consider the ultimate hot mess. Titled Beauty and the Best, the lengthy runway show featured too many concepts to mention, but here are a few. For women, Dalmau did tiny dresses with pronounced shoulders in a variety of sparkly, jangly fabrics and knits. There was also an Aztec skirt reminiscent of Proenza Schouler's 2012 collection. For men, it was all about wildly printed suiting; woolly shawls were draped over the models' shoulders.\n\nIt's hard to imagine any guy going for those capes, but the suits? There's surely a market. Because Dalmau does have a following. For those who subscribe to the Christian Audigier aesthetic, he offers fun party clothes that look great on the beaches of Miami and Spain. It's just that very few of them attend New York fashion week. Dalmau's been showing since 1997—an impressive run—but he might be better off trying, say, Madrid fashion week or even Rio on for size. Right now, his unique point of view is being ignored by the aesthetes.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                    "Gucci",
                   "",
                   "Assets/HubPage/HubPage10.png",
                   "For Spring, Frida Giannini went all in with color: azalea pink, fuchsia, and a Riviera blue that conjured sun-soaked images of the Italian jet set circa the seventies. Clearly, the Gucci designer prefers the dark side, because she's back in black for Fall, name-checking Allen Jones, the British Pop artist with the kinky streak, and juxtaposing, as her program notes put it, a demure, couture-inspired silhouette with a subversive undertone. She may be a couple weeks shy of giving birth to her first baby, but she isn't oblivious to the power of sex, that's for sure.\n\nBecause, in the end, this collection will never quite qualify as demure. How could it, with all the shiny black python she used for little numbers like a fitted skirtsuit with a slit up to there in back or a second-skin dress? Body-con and sexy, the python pieces were hands down the strongest thing about the show. The forties by way of the seventies is a look that's gaining traction this season, and the cabans, the peplum jackets, and the hourglass dresses with exaggerated hips here put Gucci in the middle of that dialogue. But some of Giannini's tailleurs in bulky astrakhan or pony hair seemed stiff, and too much of the past. It was a feeling that the fetish-y accessories—fishnets, gloves, patent oxford booties—didn't quite dispel, although the black leather and snakeskin turtlenecks she layered underneath many of the pieces added a modernizing element. Long-sleeve silk sheaths, some with bib insets of delicate black lace mesh, were in her sweet spot. The softer, suppler fabrics made a difference. At the other end of the spectrum, a boxy powder blue men's coat with a deep lapel and a fur collar had its fans.\n\nThe stretch lace mesh carried over to her evening looks—silk cocktail dresses, gowns, and jumpsuits with peekaboo bodices exuberantly embroidered in winglike patterns with sequins, paillettes, feathers, and silk fringe. Kasia Struss' knee-length cocktail number, in particular, looked like a decadent night out. Once baby comes, Giannini can do some vicarious partying when she sees them on the red carpet circuit.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-5",
                    "Jil Sander",
                   "",
                   "Assets/HubPage/HubPage11.png",
                   "The set was special: a metallic polyhedron carved into the floor. Jil said it was supposed to represent the cut of a diamond, something rare and precious. Toward the end of her show, a coat emerged, bifurcated, its top half coated in luxuriant black fur. It was immediately followed by a handful of sober wool pieces decorated with a strip of gold foil. \n\nDiamonds, fur, gold…emblems of luxury. Maybe more. You buy gold when you don't trust the future, Sander said backstage. But we're optimists. We want to believe. One thing that was quite clear here was Sander's conviction that she still has plenty to say in fashion. Yet she speaks almost too quietly. The serenity of today's presentation was unimpeachable. There was a particular skirt proportion, flaring just below the knee, that was convincingly, timelessly elegant. Jackets were elongated, slightly suppressed at the waist. Coats were mannish, reassuringly oversize. Buckled shoes with a big heel had a solid Puritan quality. All of that added up to an eminently sensible antidote to whatever is happening anywhere else in fashion. \n\nSuch is the nature of this business that you often don't know what you want till you've seen what you don't want. Jil Sander's show offered elegance, restraint, sobriety in such crisp, clearly defined terms that it could almost function as a manifesto for whatever comes next. Still, you craved the tweak. Maybe it was there in that flash of fur or splash of gold. What joy it would be if this were the start of an unexpected wingy-ness in a designer who truly has nothing left to lose in exploring the deep-rooted whys of what she does.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-6",
                    "Issa",
                   "",
                   "Assets/HubPage/HubPage12.png",
                   "The glamour girl decided to wipe off her makeup, go on a trek to the desert, and get some fresh air, Daniella Helayel said backstage at the Fall Issa show. Tapestry prints, geometric jackets, and glam wool felt fedoras with three-foot feather twills will all appeal to the Issa girl—a representation of whom appeared in the front row via the girl group the Saturdays. Yet pare it down and there was some really hard work at play: A jacquard knit turned out to be less sloppy, more structural. A heavy crepe de chine dress in a burnt orange with cut mirrors and crystals was a tour de force of embroidery. The curly Mongolian jackets and the boxy tapestry jackets all cried out for an open fire beneath the night sky.\n\nIn essence, this collection was all about the Issa woman on a spiritual retreat, cloaked in some high-luxe comfort. And all that is well and good, but don't forget that an Issa woman comes back from her retreat and re-toxes, and for that she needs some high-wattage glamour—think of the royal clients, after all. For them there were a red silk jersey gown (an Issa mainstay) and a smoldering ember-printed caftan with beaded fringe collar. Then there was a standout jade green maxi dress with gold Lurex threads that had models like Cara Delevingne and Jourdan Dunn pausing to take a closer look backstage.\n\nSince Camilla Al Fayed bought 51 percent of the company in 2011, Issa has turned into a juggernaut with no signs of abatement: The label's first shop opens in Tokyo in a few weeks, followed by Brazil in June. Helayel tells us this is her first collection that she feels is truly directional, and indeed, if that direction is toward the cash register, Issa is certainly moving fast.",
                   ITEM_CONTENT,
                   group2));
            this.AllGroups.Add(group2); 


        }
    }
}
