🇩🇪 Deutsch | 🇬🇧 [English](CONTRIBUTING.md)

# Mitarbeit an C# Deep Dive Exercises

Ein persönliches Projekt, von einer Person gepflegt.

- Issues und Pull Requests sind willkommen.
- Halte Pull Requests klein und auf eine Sache beschränkt: leichter zu prüfen,
  leichter zurückzunehmen.
- Es ist keine Vereinbarung zur Rechteübertragung nötig.
- Die Antwortzeiten schwanken; das hier läuft nicht nach Plan.

## Eine Aufgabe ergänzen oder ändern

Die folgenden Konventionen stehen in keiner einzelnen Datei. Sie zeigen sich
erst, wenn man mehrere nebeneinanderlegt, und sind deshalb hier
aufgeschrieben: für eine neue Mitarbeiterin und für eine frische
KI-Sitzung ohne Erinnerung an die vorige.

**Zwei Solutions, und sie sind nicht austauschbar**

`AnimalShelter.slnx` im Wurzelverzeichnis ist das Gerüst. Es enthält zwei
Projekte, die bauen, einen Test, der nichts prüft, und keinen Aufgabeninhalt.
So muss es bleiben. Das Gerüst ist die Ausgangslage, und Aufgabe 3.1 verlangt
eine Vorhersage darüber, was `a == b` liefert, *bevor* irgendetwas
implementiert ist. Ein Gerüst, in dem `IEquatable<Animal>` schon steht, hat
diese Antwort verschenkt.

`solution/AnimalShelter.slnx` ist die Musterlösung und die, die in der CI
getestet wird. Sie ist vollständig: jede Aufgabe, jede Zusatzaufgabe, jede
Vorhersage beantwortet.

**Jede Zahl ist gemessen**

Steht in einem Aufgabentext eine Zahl, dann gibt es in `solution/` einen Test,
der sie erzeugt. Das ist keine Formsache. Die Laufzeitaufgabe vergleicht die
Suche in einer Liste mit der in einem Dictionary bei 100 000 Einträgen, und das
gemessene Verhältnis war auf einem Rechner 2903 und auf einem anderen 563. Jede
absolute Zahl in dieser Aufgabe wäre auf mindestens einem der beiden falsch
gewesen.

Daraus folgt:

- Niemals eine Millisekundenzahl in einen Assert schreiben. Prüfe ein
  Verhältnis, und prüfe, dass das Verhältnis mit der Anzahl wächst.
- Vor dem Messen aufwärmen, sonst bezahlt der erste Aufruf den JIT-Compiler.
- Keine Elementanzahl, keine Aufrufzahl und keinen Exception-Typ aus dem
  Gedächtnis behaupten. Nachstellen, dann aufschreiben, was passiert ist.

**Die Reihenfolge ist nicht zugesagt**

`Shelter` gibt seine Tiere in der Reihenfolge heraus, in der sie zufällig in
den Körben liegen. Die folgt dem Hashcode der Chipnummer, und den setzt
`string.GetHashCode()` pro Prozess neu. Ein Test, der eine Reihenfolge
vergleicht, ohne vorher zu sortieren, läuft mit einem Startwert durch und mit
dem nächsten nicht. Deshalb läuft die Testsuite in der CI zweimal, und ein Test,
der an der Reihenfolge hängt, ist auch grün ein Fehler.

Wo die Reihenfolge Teil der Zusage *ist*, etwa bei `NamesSorted()` und
`HeaviestThree()`, darf sie geprüft werden. Dort stellt die Abfrage sie selbst
her.

**Zweisprachigkeit**

- Jede Aufgabe besteht aus zwei Dateien mit demselben Inhalt, in
  `exercises/en/` und `exercises/de/`, nicht aus einer zweisprachigen Datei.
  Die Dateinamen sind übersetzt, die Nummer davor nicht: `00-overview.md`
  gehört zu `00-ueberblick.md`.
- Fließtext außerhalb der Aufgaben folgt `NAME.md` plus `NAME.de.md`.
  Ausnahme ist `CLAUDE.md`; die Datei spricht ein Werkzeug an, keinen Leser.
- Code und Kommentare im Code sind in beiden Sprachfassungen englisch. Die
  einzige Ausnahme ist das Beispiel für einen Vorhersage-Kommentar im
  deutschen Aufgabentext: Dort schreibt die Lernende in ihre eigene Datei und
  in ihrer eigenen Sprache.
- `scripts/check-bilingual-sync.sh` erzwingt die Paarbildung. Vor dem Push
  laufen lassen.
- Eine Sprache ohne die andere zu ändern hinterlässt einen Zustand, den
  niemand beurteilen kann. Beide im selben Commit ändern.

**Aufbau eines Kapitels**

Zeile 1 ist der Sprachumschalter, Zeile 3 die Brotkrume, dann die Überschrift,
dann `**Ziel:**` beziehungsweise `**Goal:**`. Jedes Aufgabenkapitel endet mit
"Was du hier gelernt hast"; der Überblick und die Lernmaterial-Seite nicht,
weil dort nichts geübt wurde. Fließtext bricht bei etwa 80 Zeichen um.

Anders als in den Geschwister-Kursen gibt es **keine** 🟢/🟡/🔴-Marker. In einer
Aufgabe ist alles verlangt, die Zusatzaufgaben sind als Ganzes freiwillig, und
ein Marker pro Abschnitt würde eine Wahl suggerieren, die es hier nicht gibt.

**Schreiben**

Vermeide die Muster, die einen Text maschinell klingen lassen: den
Gedankenstrich als Allzweck-Verbindung, hohle Verstärker ("wirklich",
"definitiv"), mechanische Übergänge ("Außerdem", "Es ist erwähnenswert, dass"),
einen Satz, der den vorigen nur bestätigend wiederholt, und Dreiergruppen, in
denen nur zwei Elemente etwas beitragen. Das gilt beim Schreiben, nicht als
Aufräumdurchlauf danach.

**Vor dem Push**

- `dotnet build AnimalShelter.slnx` und `dotnet test AnimalShelter.slnx`, also
  das, was ein Leser als Erstes ausführt.
- `dotnet test solution/AnimalShelter.slnx` zweimal hintereinander. Null
  Warnungen, null Fehlschläge, beide Male.
- `bash scripts/check-bilingual-sync.sh`.
- Den geänderten Aufgabentext in beiden Sprachen von oben bis unten lesen. Ein
  Verweis auf "Aufgabe 4" veraltet stillschweigend, sobald etwas umnummeriert
  wird.

**Commit-Nachrichten** folgen
[Conventional Commits](https://www.conventionalcommits.org/), klein geschrieben
und im Imperativ (`feat: add exercise 4 on baskets and collisions`, `fix: ...`,
`docs: ...`, `ci: ...`).
