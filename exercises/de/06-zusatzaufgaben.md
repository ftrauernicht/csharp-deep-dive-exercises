🇩🇪 Deutsch | 🇬🇧 [English](../en/06-extras.md)

[← LINQ](05-linq.md) · [Lernmaterial](LERNMATERIAL.md)

# Zusatzaufgaben

Sieben Stück, unabhängig voneinander. Jede setzt Aufgabe 3 oder Aufgabe 4
voraus und vertieft genau eine Sache.

## Z1: `IEqualityComparer<Animal>`

*Setzt die Aufgaben 3 und 4 voraus.*

Schreib einen `ByNameEqualityComparer`, der zwei Tiere als gleich ansieht, wenn
ihre Namen übereinstimmen, ohne Beachtung der Groß- und Kleinschreibung.

Benutz ihn mit `new HashSet<Animal>(comparer)` und mit `Distinct(comparer)`.

Gib `BasketDictionary` einen Konstruktor, der einen `IEqualityComparer<TKey>`
annimmt, und zeig in einem Test, dass derselbe Schlüssel je nach Vergleicher
gefunden wird oder nicht.

Der Gewinn ist größer, als er aussieht: `Animal.Equals` legt **eine** Bedeutung
von Gleichheit fest, für immer und für alle. Ein Vergleicher von außen
erlaubt eine zweite, ohne die Klasse anzufassen. Überleg, wann du welches von
beidem willst.

## Z2: Änderung während der Aufzählung

*Setzt Aufgabe 4 voraus.*

`BasketDictionary` wirft eine `InvalidOperationException`, wenn es während
eines `foreach` verändert wird.

Der übliche Weg ist ein Zähler, der bei jeder strukturellen Änderung
hochgezählt und beim Aufzählen gegengeprüft wird.

Die eigentliche Frage ist, **was als strukturelle Änderung zählt**. Einen
Schlüssel hinzufügen offensichtlich. Einen entfernen auch. Aber einen
vorhandenen Wert zu überschreiben? Das `Dictionary` aus .NET erlaubt es
während der Aufzählung. Entscheide dich, begründe es, und schreib für beide
Fälle einen Test.

Ein Hinweis aus leidvoller Erfahrung: Wenn du deine Körbe mit `foreach` über
die innere `List<T>` durchläufst, entscheidet nicht mehr dein Zähler, sondern
der von `List<T>`. Und `List<T>` zählt eine Indexer-Zuweisung als Änderung.
Dein Dictionary verhält sich dann anders, als dein eigener Code behauptet.

## Z3: Verkettete Liste

*Setzt Aufgabe 4 voraus.*

Ersetz die `List<KeyValuePair<...>>` in den Körben durch eine eigene einfach
verkettete Liste: eine Klasse `Entry` mit `Key`, `Value` und `Next`.

Alle Tests aus Aufgabe 4 müssen unverändert grün bleiben.

Das Lehrreiche steckt im Entfernen. Eine `List<T>` erledigt `RemoveAt` für
dich; in einer verketteten Liste muss das Glied **davor** auf das Glied
**danach** zeigen. Das erste Glied eines Korbs hat keinen Vorgänger. Das sind
zwei Fälle, und wer nur einen davon testet, merkt es nicht.

Schreib deshalb gezielt drei Schlüssel mit gleichem Hashcode in einen Korb und
entferne einmal den ersten, einmal den mittleren, einmal den letzten.

## Z4: Laufzeit

*Setzt Aufgabe 4 voraus.*

Vergleich `BasketDictionary`, `Dictionary` und eine `List<KeyValuePair<...>>`
mit linearer Suche bei 100, 10 000 und 100 000 Einträgen.

**Schreib keine Millisekundenzahl in einen Assert.** Sie gilt auf deinem
Rechner, zu dieser Tageszeit, bei diesem Akkustand. Was überall gilt, ist das
Verhältnis. Noch belastbarer ist, dass es mit der Anzahl **wächst**.

Miss, drucke eine Tabelle, und formulier dann einen Assert, der auf jedem
Rechner hält. Wärm vor dem Messen einmal auf, sonst misst du den JIT-Compiler.

Bei 100 Einträgen wirst du kaum einen Unterschied sehen. Das ist kein
Messfehler. Ab wann es nicht mehr stimmt, misst du hier selbst.

## Z5: Records

*Setzt Aufgabe 3 voraus.*

Eine `record ChipNumber(string Value)` tritt an die Stelle des `string` in
`Animal`.

Teste, **welche Methoden der Compiler bei einem `record` automatisch erzeugt**,
und leg das Ergebnis neben das, was du in Aufgabe 3 von Hand geschrieben hast.
Wertgleichheit, Hashcode, `ToString`, `Deconstruct`, `==` und `!=`, dazu
`with`.

Danach die unbequeme Frage: Wenn der Compiler das alles kann, warum hast du
Aufgabe 3 dann überhaupt gemacht? Die Antwort hat zwei Teile, und der zweite
ist der wichtigere: ein `record` erzeugt Gleichheit über **alle** Felder, und
in Aufgabe 3 sollte genau das nicht gelten.

> Die Musterlösung lässt `Animal` beim `string` und stellt den Record daneben.
> Der Umbau würde jede Signatur aus den Aufgaben 1 bis 5 ändern. Mach ihn auf
> einem Zweig, wenn du ihn ganz sehen willst.

## Z6: Ein zweiter Indexer

*Setzt Aufgabe 4 voraus.*

`Shelter` bekommt zusätzlich `this[int kennelNumber]`. Jedes Tier erhält bei
der Aufnahme die nächste freie Box; nach einer Adoption wird die Box wieder
frei.

Zwei Indexer an einer Klasse werden am Typ in den Klammern unterschieden:
`shelter["2760981..."]` ist eine Chipnummer, `shelter[1]` ist eine Box.

Entscheide, was "die nächste freie Box" heißt, wenn Box 1 frei wird und Box 2
belegt ist: zählt der Zähler weiter, oder wird die Lücke gefüllt? Beides ist
vertretbar, und der Test hält fest, wofür du dich entschieden hast.

## Z7: Adoptionsanfragen

*Setzt Aufgabe 3 voraus.*

Anfragen werden nach Eingangsdatum in einem `SortedSet<AdoptionRequest>`
verwaltet.

### Vorhersage

Was passiert bei **zwei Anfragen mit identischem Datum**, wenn `CompareTo` nur
das Datum vergleicht?

Sag es voraus, dann bau es. Und wenn du das Ergebnis siehst: Es gibt keine
Fehlermeldung. `Add` liefert einen Rückgabewert, den fast niemand ansieht.

Korrigier das Verhalten. Die Korrektur ist dieselbe Idee wie in Aufgabe 3.4:
ein Vergleich, der nur dann 0 liefert, wenn zwei Dinge wirklich dasselbe sind.

## Was du hier gelernt hast

- Gleichheit von außen bestimmen, statt sie in der Klasse festzuschreiben
- Was eine strukturelle Änderung ist und was nur eine Wertänderung
- Zeiger umhängen, und warum das erste Glied ein Sonderfall ist
- Eine Messung formulieren, die auf einem fremden Rechner noch stimmt
- Was ein `record` schenkt, und wann das Geschenk nicht passt
- Zwei Indexer, unterschieden am Typ
- Eine Datenstruktur, die stillschweigend Einträge verschluckt, wenn der
  Vergleich zu grob ist
