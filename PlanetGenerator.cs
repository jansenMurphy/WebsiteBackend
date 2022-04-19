using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteBackend{
public class PlanetGenerator{
    private string[] planets = {"Hoxxes", "Ludem", "Vulpes", "Fenris", "Terra", "Ecaz", "Tocatta", "Vanaheim", "Echo", "Mombasa", "Terim", "Vanguard", "Utraxis", "Klom", "Florence", "Crystal", "Parnath", "Neverwinter", "El Dorado", "Heartache", "Harlock", "Chime", "Constantinople", "Adrianople", "Alexandria", "Cyre", "Tartarus", "Cardassia", "Harbinger", "Galatea", "Arquebus", "Ooo", "Cholula", "Alabaster", "Horus", "Masquerade", "Jericho", "Arha", "Risa", "Noel", "Tidings", "Macuahuitl", "Atlantis", "Troy", "Aether", "Bryyo", "Caprica", "Tallon", "Askellon", "Malayoh", "Cassini", "Goddard", "Galileo", "Rosen","Petchorin","Asimov","Turgenev","Severnaya","Mazurka","Chariot"},
    suffixes = {" I", " II", " III", " IV", " V", " VI", " Novum", " Major", " Minor", " Antiquum", " Palace", " Rex", " Indominus", " of the eight rings", " of the golden haze", " blessed by the emperor", " the unlucky", " VII", " IX", " XI", " Draconorum", " Bellarum", " Probitatis", " VIII", " X", " Prime", " Alpha", " Beta", " Gamma", " Delta", " Omega", " G", " K", " L", " N", " M", " R", " Q", " W", " Z", " Epsilon", " Audaciae", " Cupidi", " Obstinatus", "Exsilii", " Externa", " Portarum", " Hastarum", " Lacrimorum", " Forlorn", "'s Vengeance", "'s Mercy", "'s Wrath", "' Peace", "' Tranquility", "'s Wager", "' Strength", "'s Gambit", "'s Delight", "'s Tempest", "'s Piety", "'s Loss", "", "","","","","","",""},
    occupations={"rare metal extraction", "xenobiological husbandry", "farming staple crops", "shipbuilding", "luxury good creation", "research", "imperial administration", "heavy machinery manufacture", "computer and microchip manufacture", "engineering education", "data analysis", "liberal arts education", "mining", "chemical synthesis", "colonizinization", "cultural exports", "resolving temporal anomalies","training soldiers", "AI cultivation", "trade", "hydrogen skimming", "peace and meditation", "clarktech R&D", "synthetic crystal growth", "nothing", "stellar cartography", "propoganda", "water ice aggregation", "exotic horticulture", "bioengineered machines", "tourism", "cultivating psyonics", "teaching the ancient sacred martial art of Sho'ir'ibesu", "animal breeding", "weapon manufacture", "mutagenics", "biological R&D", "farming staple crops", "trade", "nothing", "math education", "training spacers", "shipbuilding", "mining", "mining", "catching asteroids", "orgone accumulation", "ore refining", "manufacuring precise mechanisms", "training soldiers", "xenolinguistics", "media creation", "mysterious disappearances", "a broken economy", "medical technology", "rampant violence", "selling snake oil", "selling actual oil for alien snakes", "physics research"},
    locations={"water world", "asteroid belt", "gas giant's moon", "planet with a perfectly circular orbit", "planet with a stable trinary orbit", "ringworld", "iceball", "tidally locked world", "world orbiting a blue/white binary stars", "planet undergoing active terraforming", "planet filled with alien artifacts", "desert planet", "ecumenopolis", "garden world", "Minshara class planet", "dwarf planet with inexplicably earthlike atmosphere and gravity", "planet full of apex predators", "culturally important home of the recently deceased emperor", "recently opened quarantine world", "planet with extremely eccentric orbit", "rogue planet", "Dyson sphere","planet with two parallel dimensions","high-atmosphere balloon station","volcanically active lava ball", "planet in reverse orbit", "planet with reversed spin", "long-abandoned alien colony", "recently rediscovered planet", "geometrically impossible alien superstructure", "mostly hollow moon", "moon with vast underground ocean", "planet with thick methane atmosphere", "large rocky world", "planet largely hidden by a nebula", "perfect sphere with evenly spaced circular islands", "radiation-ridden world", "wartorn moon", "moon of a moon of a hypergiant", "pocket dimension", "ball of grey nanite goo", "binary planet system", "world in a trinary red dwarf system", "ghost world recently hit by a gamma ray burst", "planet hit by regular CMEs", "planet with 4 hour days", "planet with extreme axial tilt", "cluster of small inhabited moons", "world-spanning macroorganism", "planet with ruins of aliens that failed to develop interstellar travel", "dystopic corporate world", "world previously independant from the empire", "paradise world", "world with a giant chunk missing", "Minshara class planet", "earthlike planet", "earthlike planet", "earthlike planet", "artifical satellite", "artifical satellite"},
    faction={"Imperial Loyalists", "Spacing Guild", "Xenosapient Superintelligence", "Populist Mob", "Secret Police", "Samurai Wizard Mystics", "Cult of the Precursors", "Seditionists", "Organic Hive Mind", "Opportunist Admirals", "Organized Crime", "Pirates", "Merchant Conglomerate"};

    private List<int> planetIDs,suffixIDs,occupationIDs,locationIDs, factionIDs;

    public PlanetGenerator(){
        planetIDs = new List<int>();
        suffixIDs = new List<int>();
        occupationIDs = new List<int>();
        locationIDs = new List<int>();
        factionIDs = new List<int>();

        for(int i=0;i<planets.Length;i++){
            planetIDs.Add(i);
        }
        for(int i=0;i<faction.Length;i++){
            factionIDs.Add(i);
        }
        for(int i=0;i<suffixes.Length;i++){
            suffixIDs.Add(i);
        }
        for(int i=0;i<locations.Length;i++){
            locationIDs.Add(i);
        }
        for(int i=0;i<occupations.Length;i++){
            occupationIDs.Add(i);
        }

        Shuffle(planetIDs);
        Shuffle(suffixIDs);
        Shuffle(occupationIDs);
        Shuffle(locationIDs);
        Shuffle(factionIDs);
    }


    public Planet MakePlanet(){
        string _name, _description;
        StringBuilder sb = new StringBuilder();
        sb.Append(planets[planetIDs[0]]);
        sb.Append(suffixes[suffixIDs[0]]);
        _name = sb.ToString();
        if(Regex.Match(locations[0].Substring(0,1), "[aeiou]") is null){
            sb.Append(" is a ");
        }else{
            sb.Append(" is an ");
        }
        sb.Append(locations[locationIDs[0]]);
        sb.Append(" specializing in ");
        sb.Append(occupations[occupationIDs[0]]);
        sb.Append(".");
        _description = sb.ToString();
        planetIDs.RemoveAt(0);
        suffixIDs.RemoveAt(0);
        occupationIDs.RemoveAt(0);
        locationIDs.RemoveAt(0);
        return new Planet(){name=_name,description=_description};
    }

    public string MakeFaction(){
        string retval = faction[factionIDs[0]];
        factionIDs.RemoveAt(0);
        return retval;
    }

    private void Shuffle<dType>(List<dType> list){
        Random rand = new Random();
        for(int i = list.Count-1; i>0; i--){
            dType temp = list[i];
            int randIndex = rand.Next(i+1);
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
}
}