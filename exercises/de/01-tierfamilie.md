🇩🇪 Deutsch | 🇬🇧 [English](../en/01-animal-family.md)

[← Überblick](00-ueberblick.md) · Weiter: [Tierarzt und Interfaces](02-tierarzt-und-interfaces.md)

# Aufgabe 1: Die Tierfamilie

**Ziel:** eine Basisklasse, von der drei Tierarten erben, und eine Methode, die
mit allen dreien rechnet, ohne eine einzige davon beim Namen zu nennen.

## 1.1 Die abstrakte Basisklasse `Animal`

In `AnimalShelter.Core.Animals`.

| Mitglied | Art |
|---|---|
| `Name` | `string`, get und set |
| `ChipNumber` | `string`, nur get, wird im Konstruktor gesetzt |
| `AgeInMonths` | `int`, get und set |
| `WeightInKg` | `double`, get und set |
| `MakeSound()` | `abstract`, liefert `string` |
| `CalculateDailyFoodInGrams()` | `virtual`, liefert `double`, Standard: 30 g pro Kilogramm |
| `Describe()` | `virtual`, liefert `string`, Standard: `"{Name}, {AgeInMonths} months"` |
| `ToString()` | `override`, gibt `Describe()` zurück |

Der Konstruktor ist `protected` und nimmt die vier Werte in dieser
Reihenfolge:

```csharp
protected Animal(string name, string chipNumber, int ageInMonths, double weightInKg)
```

Halt dich an die Reihenfolge. Jede weitere Aufgabe legt Tiere an, und die
Musterlösung tut es mit genau dieser Signatur.

`ChipNumber` ist das einzige Mitglied ohne Setter, und das ist kein Versehen:
Ein Tier wird schwerer und älter, aber es bekommt keine neue Chipnummer. Was
passiert, wenn man das anders sieht, steht in Aufgabe 3.

Der Konstruktor wirft eine `ArgumentException`, wenn

- `Name` leer ist oder nur aus Leerzeichen besteht,
- `ChipNumber` nicht aus genau 15 Ziffern besteht,
- `AgeInMonths` negativ ist,
- `WeightInKg` nicht größer als 0 ist.

Zwei Grenzen, über die man leicht hinwegliest: Ein Alter von **0** Monaten ist
gültig, denn ein neugeborenes Tier ist kein Fehler. Und ein Gewicht von 0 ist
ungültig, nicht erst ein negatives.

## 1.2 Die konkreten Tiere

| Klasse | `MakeSound()` | Futter pro Tag | Zusätzlich |
|---|---|---|---|
| `Cat` | `"Meow"` | 40 g/kg, unter 12 Monaten das Doppelte davon, also 80 | `IsOutdoor` (`bool`) |
| `Dog` | `"Woof"` | Standard der Basisklasse | `Breed` (`string`) |
| `Rabbit` | `"..."` | 50 g/kg | — |

Der Laut des Kaninchens ist wörtlich gemeint. Ein Kaninchen ist leise.

Die Spalte "Zusätzlich" meint eine Eigenschaft mit Konstruktorparameter: `Cat`
bekommt `isOutdoor` dazu, `Dog` bekommt `breed`. Beide kommen hinter die vier
Werte der Basisklasse.

`Cat` überschreibt `Describe()` und hängt `" (outdoor)"` beziehungsweise
`" (indoor)"` an. Benutz dafür `base.Describe()` und bau die Zeichenkette nicht
noch einmal zusammen. Sonst stehen zwei Stellen da, die dasselbe Format
kennen, und beim nächsten Mal ändert jemand nur eine davon.

Beachte, dass `Dog` beim Futter nichts überschreibt. `MakeSound()` muss er
liefern, weil die Methode `abstract` ist und der Compiler sonst nicht
übersetzt; beim Futter dagegen hat er die Wahl und nimmt den Standard. Genau dazu hat
eine `virtual`-Methode einen Standard.

## 1.3 Der Futterplan

Eine statische Klasse `FeedingPlan`, ebenfalls in `AnimalShelter.Core.Animals`,
mit einer statischen Methode:

```csharp
public static double TotalDailyFoodInGrams(IEnumerable<Animal> animals)
```

**Die Methode enthält kein `if`, kein `is` und kein `switch` auf den Tiertyp.**

Das ist die Aufgabe, und keine Stilregel. Jedes Tier kennt
seinen Futterbedarf bereits; eine Methode, die danach fragt, bleibt richtig,
wenn morgen eine fünfte Tierart dazukommt. Eine Methode, die den Typ abfragt,
muss dann geändert werden — und zwar von jemandem, der weiß, dass es sie gibt.

## 1.4 Tests

- `MakeSound()` für jede Tierart, geprüft über eine konkrete Klasse: `Animal`
  ist abstrakt, `new Animal(...)` übersetzt nicht, und auch die
  Validierungsregeln prüfst du deshalb an einer Tierart deiner Wahl
- Futterbedarf: erwachsene Katze, Kitten, Katze mit **genau 12 Monaten**, Hund,
  Kaninchen
- jede einzelne Validierungsregel
- `Describe()` für Freigänger und Wohnungskatze
- `TotalDailyFoodInGrams` mit einer gemischten Liste
- ein Test, der eine `List<Animal>` mit Katze, Hund und Kaninchen durchläuft und
  alle Laute einsammelt

Die Katze mit genau zwölf Monaten steht nicht zufällig da. Die Regel sagt
"unter 12 Monaten das Doppelte", bei genau zwölf gilt sie also nicht mehr, und
diese Grenze ist die Stelle, an der sich ein `<` und ein `<=` unterscheiden.

**Alle Tiere werden in Variablen vom Typ `Animal` gespeichert**, nicht vom
konkreten Typ. Nur dort, wo eine art-spezifische Eigenschaft wie `IsOutdoor`
geprüft wird, darf der konkrete Typ stehen. Der Grund wird in 1.5 sichtbar.

## 1.5 Vorhersage: `new` statt `override`

Gib `Rabbit` vorübergehend diese Methode:

```csharp
public new string Describe() => $"Rabbit {Name}";
```

Schreib Tests für:

```csharp
Rabbit a = new Rabbit(...);
Animal b = new Rabbit(...);
```

Was liefern `a.Describe()`, `b.Describe()` und `b.ToString()`?

Trag deine drei Vorhersagen als Kommentare ein, bevor du den Test ausführst.
Besonders die dritte lohnt sich: `ToString()` steht in `Animal` und ruft von
dort aus `Describe()` auf.

Ersetz danach `new` durch `override` und führ dieselben Tests erneut aus. Pass
die Asserts an und lass die alten Vorhersagen als Kommentar stehen.

Die Methode bleibt dann stehen, als `override`. Das ist der Endzustand dieser
Aufgabe, und die Musterlösung sieht genauso aus: `Rabbit` beschreibt sich ab
hier als `"Rabbit Hoppel"`.

### Warum das zweite Ergebnis anders aussieht

Bei `new` entscheidet der Typ, der **im Quelltext** steht. `Rabbit a` ruft die
Methode aus `Rabbit` auf, `Animal b` die aus `Animal`, und beide Aufrufe stehen
schon zur Übersetzungszeit fest. Dasselbe Objekt antwortet unterschiedlich, je
nachdem, durch welche Brille man es ansieht.

Bei `override` entscheidet das Objekt. `Animal` trägt für `Describe()` einen
Eintrag, den eine Ableitung überschreiben kann, und der Aufruf schlägt zur
Laufzeit dort nach.

`ToString()` ist der Fall, der es zeigt. Die Methode steht in `Animal` und
kennt `Rabbit` nicht einmal. Mit `new` findet sie deshalb unmöglich die Methode
aus `Rabbit`; mit `override` findet sie genau sie. Deshalb ist `new` fast nie
das, was man will: Es sieht wie Überschreiben aus und wirkt nur dort, wo der
statische Typ zufällig passt.

## Was du hier gelernt hast

- Eine abstrakte Basisklasse, die einen Standard mitbringt, den nicht jeder
  erben muss
- `abstract` gegen `virtual`: das eine erzwingt eine Antwort, das andere bietet
  eine an
- `base.Describe()` statt derselben Zeichenkette an zwei Stellen
- Eine Methode über eine gemischte Liste, die keinen einzigen Typ kennt
- Was `new` an einer Methode tut, und warum dabei der statische Typ entscheidet
