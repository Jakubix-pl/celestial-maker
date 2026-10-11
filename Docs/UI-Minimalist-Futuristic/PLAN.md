# Celestial Maker — plan minimalistycznego, futurystycznego UI

Data analizy: 11 października 2026. Punkt wyjścia: działająca wersja 2.1.0
i kampania skalibrowana na około 400 godzin. Status: **projekt i makieta**.
W tym etapie nie wdrażano zmian interfejsu do Unity ani zmian balansu.

## 1. Rekomendacja

**Orbitalne obserwatorium:** spokojne powierzchnie, czytelne liczby, precyzyjne
wyrównanie, dyskretne orbity i pojedynczy turkusowy akcent dla działań. Widok
produkcji jest miejscem podejmowania decyzji. Ilustracja kosmosu ma największą rolę
w menu, podsumowaniu rozdziału i prezentacji megastruktur.

Najważniejsze pytania gracza powinny mieć odpowiedź na bieżącym ekranie:

1. Ile mam i ile przybywa netto?
2. Co mogę kupić i jaki będzie efekt?
3. Co ogranicza produkcję i gdzie mogę to poprawić?
4. Jaki jest najbliższy cel?
5. Co zrobi automat lub reset?

Proponowany wariant domyślny: **Obserwatorium**, ze stałą nawigacją po lewej.
Alternatywa **Terminal** porównuje poziomą nawigację i większą gęstość wierszy.
To dwa warianty do oceny; wdrożenie jednego spójnego układu ograniczy koszt obsługi
i testowania. Nie trzeba utrzymywać dwóch niezależnych interfejsów.

## 2. Analiza popularnych gier

Popularność oszacowano przez **liczbę anglojęzycznych recenzji na Steam**, odczytaną
z kart produktów. To porównywalny wskaźnik rozpoznawalności w tej próbie, a nie
liczba sprzedanych kopii, aktualnych graczy ani globalny ranking całego gatunku.
Wybór obejmuje duże klasyki oraz bliższe mechanicznie gry wielowarstwowe.
Cookie Clicker, AdVenture Capitalist i Clicker Heroes stanowią główne punkty
odniesienia popularności; Antimatter Dimensions jest szczególnie bliskie strukturą.

| Gra / źródło | Recenzje EN | Pozytywne EN | Wniosek dla Celestial Maker |
|---|---:|---:|---|
| [Cookie Clicker](https://store.steampowered.com/app/1454400/Cookie_Clicker/) | 54 756 | 96% | Stała pozycja zasobów i zakupów pomaga utrzymać orientację; koszt i liczba zakupów są blisko siebie. |
| [AdVenture Capitalist](https://store.steampowered.com/app/346900/AdVenture_Capitalist/) | 38 532 | 86% | Powtarzalny układ biznesów oraz osobne zarządzanie automatami dają prosty początek. U nas: ten sam rytm dla każdego procesu. |
| [Clicker Heroes](https://store.steampowered.com/app/363970/Clicker_Heroes/) | 28 674 | 88% | Kupowanie wielu poziomów i widoczny koszt przy każdym wierszu. Warto zachować wspólny przełącznik liczby zakupów. |
| [IdleOn](https://store.steampowered.com/app/1476970/IdleOn__The_Idle_RPG/) | 21 848 | 75% | Opis gry podkreśla wiele postaci i rozwijanych aktywności. Dla naszych równoległych gospodarek ważne będą porównanie stanu i jasny wybór aktywnego wszechświata. To odniesienie systemowe, nie audyt układu ekranu. |
| [NGU IDLE](https://store.steampowered.com/app/1147690/NGU_IDLE/) | 10 921 | 95% | Stałe menu i odkrywanie funkcji wspierają długi rozwój. Rozbudowany pasek funkcji pokazuje też ryzyko przeciążenia nawigacji. |
| [Melvor Idle](https://store.steampowered.com/app/1267910/Melvor_Idle/) | 8 626 | 92% | Kategorie, widoczny wybór aktywności i osobne stany zablokowane. Przydatne przy grupowaniu wielu mechanik Celestial Maker. |
| [Revolution Idle](https://store.steampowered.com/app/2763740/Revolution_Idle/) | 7 513 | 86% | Natychmiastowy wizualny sygnał postępu i widoczna następna warstwa. U nas wystarczy mały wskaźnik celu oraz motyw orbitalny. |
| [(the) Gnorp Apologue](https://store.steampowered.com/app/1473350/) | 7 408 | 95% | Prosty język wizualny potrafi nadać grze mocny charakter. Odniesienie do atmosfery i widocznej aktywności świata, nie wzór analityki. |
| [Antimatter Dimensions](https://store.steampowered.com/app/1399720/Antimatter_Dimensions/) | 3 413 | 91% | Wyrównane wiersze, zakup przy każdym wymiarze, osobna automatyzacja i warstwy rozwoju pasują do wielkich liczb i wielu resetów. |

Źródła ocen: karty Steam odczytane w tej sesji. Źródła wyglądu: oficjalne materiały
promocyjne pobrane przez publiczne metadane Steam; manifest adresów i czasów
odczytu: `Logs/UIPlanning/References/sources.json`. Zrzuty promocyjne mogą
przedstawiać wcześniejsze wydania. Nie uruchamiano konkurencyjnych gier. Materiały
IdleOn i Gnorp służą głównie jako odniesienie stylistyczne/systemowe.

Oceny użyteczności poniżej są **wnioskami projektowymi z analizy**, nie wynikami
testów z użytkownikami ani dowodem, że konkretne UI spowodowało sukces gry:

- Spójne kolumny ułatwiają porównanie kosztów i wydajności. Wybieramy wiersze
  produkcji, a karty zostawiamy dla niewielu, odmiennych decyzji.
- Stała nawigacja pomaga po długiej przerwie. Dużo funkcji wymaga grupowania oraz
  stopniowego ujawniania, a nie kilkunastu równorzędnych przycisków.
- Automatyzacja powinna pokazywać stan i przyczynę oczekiwania. Sam przełącznik
  nie wyjaśnia, dlaczego produkcja lub budowa stoi.
- Reset wymaga własnego słownictwa, nagrody i listy konsekwencji. Kolor może
  wspierać identyfikację warstw; nazwa i opis pozostają zawsze widoczne.
- Wyrazisty motyw może zajmować mało miejsca. Orbity i proporcje będą wystarczające
  dla futurystycznego charakteru podczas codziennego zarządzania.

## 3. Co należy uporządkować w obecnym UI

Audyt opiera się na źródłach, nie na uruchomionym graczu Unity:

- `GameUI.PC.cs` deklaruje 17 stron na głównym poziomie. Część jest warunkowo
  ukryta, ale nadal powstaje długa lista podobnych celów: Overview, Roadmap,
  Analytics, Automation oraz osobne strony trzech mechanik kosmicznych.
- Karty procesu mają wysokość 420 jednostek przy siatce 2–3 kolumn. Koszt,
  przepływ i zakup są trudniejsze do porównania między wieloma procesami niż
  w wyrównanych wierszach. To ryzyko wynikające ze struktury, nie zmierzony błąd UX.
- Panorama Overview ma wysokość 236 jednostek, obok powtarzanych informacji
  o horyzoncie i kampanii. Najbliższa decyzja powinna znaleźć się wyżej.
- Boczny panel kontekstowy często przedstawia ogólny horyzont; wykorzystamy go
  na aktualnie wybrany proces, projekt lub węzeł.
- Część tekstów pomocniczych ma 14–16 jednostek, stopka może zmniejszać się do 11.
  Docelowo ważne wyjaśnienia i liczby nie będą ratowane automatycznym zmniejszaniem.
- Obecna ilustracja i przyciski złote definiują wcześniejszy styl. Nowy zestaw
  komponentów wykorzysta spokojne, nieprzezroczyste powierzchnie i akcent turkusowy.

## 4. Architektura informacji — sześć sekcji

| Główna sekcja | Zawartość | Obecne funkcje / strony |
|---|---|---|
| Przegląd | Następna decyzja, istotne ograniczenie, ostatnia nieobecność | Overview, skrót Analytics, odnośnik do Roadmap i Offline summary |
| Produkcja | Procesy, Badania, Przepływy, rezerwy | Production, Research, Analytics |
| Rozwój | Resety, drzewka i specjalizacje, kamienie milowe, profile, wyzwania | Prestige, część Constellations, Roadmap, Challenges |
| Automatyzacja | Zakupy, badania, budowa, resety i ekspedycje: reguła + stan | Automation i obecne rozproszone przełączniki |
| Kosmos | Megastruktury, prawa, wszechświaty, połączenia, ekspedycje | Megastructures, Reality laws, Parallel universes, linki Constellations, Expeditions |
| Dziennik | Osiągnięcia, kodeks, zachowane raporty i rekordy | Achievements, Codex, Offline summary |

Ustawienia i zapis/import/eksport pozostają w stałym obszarze narzędziowym.
Historia raportów oznacza na początku obecny ostatni raport; archiwum wielu
raportów wymaga osobnej decyzji o danych i nie jest ukrytą zmianą zakresu.

Początek gry: Przegląd, Produkcja, podstawowa Automatyzacja i Ustawienia.
Kolejne grupy ujawniamy zgodnie z istniejącymi warunkami odblokowania, a nie
według arbitralnej liczby godzin. Pokazujemy jeden najbliższy zablokowany cel
z warunkiem. Ujawnione pozycje zachowują kolejność.

Zmiana wszechświata jest podpisanym wyborem w górnym pasku po odblokowaniu tej
funkcji. W widokach wyłącznie globalnych widnieje etykieta „Globalne”, aby uniknąć
sugestii, że reset lub drzewko dotyczy tylko zaznaczonego satelity.

## 5. Układ i najważniejsze ekrany

Wariant Obserwatorium, punkt odniesienia 1920 × 1080:

- Górny pasek 80–88 px: nazwa gry, kontekst wszechświata i 3–4 istotne zasoby.
- Lewa nawigacja 200–216 px: nazwa i prosta ikona; wyraźne aktywne położenie.
- Główny obszar: elastyczny, z wyrównanymi kolumnami i lokalnymi zakładkami.
- Panel szczegółów 300–340 px: pojawia się dla wybranego elementu. Przy mniejszej
  przestrzeni otwiera się jako szuflada lub sekcja rozwijana pod wierszem.
- Stopka 28–36 px: zapis, tryb działania, zwięzły komunikat. Ważny błąd zapisu
  dostaje trwałe powiadomienie i możliwość ponowienia.

**Produkcja.** Wiersz 88–104 px: nazwa, liczba jednostek, rzeczywista produkcja,
widoczny status, łączny koszt wybranego pakietu i zakup. Jedna kontrola 1 / 10 /
Max / następny próg dla całej listy. Stałe wyrównanie liczb, zachowanie pozycji
przewijania i zaznaczenia po zakupie. Wyłączony zakup opisuje brakujący zasób.
Brak samoczynnego sortowania co klatkę. Dostępne filtry: etap, dostępne zakupy,
ograniczone procesy. Panel szczegółów rozróżnia moc instalacji od realnej produkcji.

**Badania.** Efekt przed / po, koszt, wymaganie i stan. Zakup na miejscu.
Filtry Wszystkie / Dostępne / Kupione. Długie wzory i pełne mnożniki dopiero
w szczegółach, bez ukrywania podstawowej korzyści.

**Rozwój.** Prestige / Ascension / Genesis mają własne nagłówki, koszt wejścia
i nagrodę z Core. Kliknięcie otwiera dwie kolumny „Zostaje” oraz „Resetuje się”.
Potwierdzenie nazywa operację. Żaden skrót nie uruchamia dużego resetu bez tego
podglądu. Zamknięcie zachowuje kontekst. Czas odzyskania produkcji może być
pokazywany tylko jako oszacowanie z ujawnionym założeniem, nigdy jako gwarancja.

**Drzewka.** Energy / Matter / Time jako trzy uporządkowane kolumny z czytelnym
stanem połączeń i kosztami. Genesis jako osobna warstwa stałych fundamentów.
Lista węzłów jest równorzędną alternatywą dla mapy, szczególnie przy skali 200%.
Podgląd profilu pokazuje przesuwane punkty i efekt produkcji przed zastosowaniem.

**Automatyzacja.** Każda reguła: nazwa, warunek, działanie, Włączona/Wyłączona,
stan „działa / czeka / zablokowana” i powód. Rezerwy są widoczne obok reguł,
które je respektują. Planista opisuje cel 1 przed Endless Horizon i 6 po nim.
Przy przygotowaniu Genesis panel przypomina o zatrzymaniu niższych resetów.
Włączenie automatycznego resetu pokazuje jego konsekwencje raz przy zmianie reguły.

**Kosmos.** Megastruktury: trzy porównywalne projekty z poziomem, kosztem trzech
surowców, zajętym budżetem, korzyścią i czasem do ukończenia przy obecnej prędkości.
Prawa: trzy rodziny, aktywne wybory i czas blokady zmiany. Wszechświaty: wspólna
tabela porównania, osobne stany gospodarek, jawne źródło/cel transferu i strata 20%.

**Przegląd i powrót po przerwie.** Jedno działanie o najwyższym priorytecie,
jedno istotne ograniczenie, najbliższy cel. Raport pokazuje przyrost netto oraz
rozkład „wyprodukowano / wydano / utracono przy resecie / nagrody”. Ujemny bilans
ma wyjaśnienie. Podczas nadrabiania zaległości widać postęp przeliczenia; stan
przycisków wynika z tego, czy Core może już przyjąć działanie.

**Menu i finał.** W menu jedna spokojna ilustracja orbitalna, „Kontynuuj” jako
główne działanie, czas kosmosu i ostatni etap. Finał pokazuje spełnione 9 węzłów
Ascension, 3 fundamenty Genesis, 3 wszechświaty i 18 etapów megastruktur wraz
z rzeczywistym czasem ukończenia. Dalsza gra pozostaje dostępna. Nie używamy
licznika „400 godzin pozostało” ani procentu czasu jako procentu ukończenia.

## 6. Język wizualny

| Rola | Motyw Noc | Zastosowanie |
|---|---|---|
| Tło | `#0B1118` | Przestrzeń aplikacji |
| Powierzchnia | `#121C27` | Nawigacja i szczegóły |
| Stan uniesiony | `#1A2836` | Zaznaczenie i hover |
| Tekst główny | `#EDF4FA` | Nazwy, wartości i działania |
| Tekst pomocniczy | `#A5B7C8` | Jednostki, opisy, wymagania |
| Działanie | `#71DFD4` | Główna akcja, aktywny stan |
| Ostrzeżenie | `#F0C178` | Brak wejścia, oczekiwanie z powodu zasobu |
| Warstwa rozwoju | `#C1A5EF` | Delikatne oznaczenie rozwoju meta |
| Linia strukturalna | `#354858` | Rozdzielenie sekcji, nie jedyna granica kontrolki |
| Granica kontrolki | `#748B9F` | Czytelne obszary interakcji |

Docelowa baza: istniejący **IBM Plex Sans** do tekstu i etykiet; cyfry o stałej
szerokości w porównaniach i licznikach. Rajdhani może zostać wyłącznie w logotypie.
Tytuły 28–36 px, tekst i etykiety 18 px przy 1080p, liczby zasobów 26–32 px.
Mikroetykiety 14–16 px tylko przy nieistotnych metadanych. Pełne nazwy po polsku
i po angielsku muszą się mieścić bez zmniejszania kluczowego tekstu.

Siatka odstępów 8 px, narożniki 6–8 px, kontrolki 42–48 px, cienkie linie.
Brak animacji pod ważnymi liczbami. Przejścia 120–180 ms; przy ograniczeniu ruchu
natychmiastowa zmiana stanu. Orbity mają sugerować skalę i porządek świata.

Motyw Świt zachowuje tę samą geometrię z jasnymi powierzchniami i ciemnym tekstem.
Makieta dziedziczy jasny/ciemny motyw hosta; dostępne jest porównanie Noc/Świt.
Nie zawiera assetów graficznych konkurencji.

## 7. Czytelność, obsługa i adaptacja

Założenia opierają się na [Xbox Accessibility Guideline 101](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101)
(dla PC m.in. minimum 18 px wysokości tekstu przy 1080p oraz możliwość skalowania)
i [wyjaśnieniu WCAG 1.4.3](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html)
(co najmniej 4,5:1 dla zwykłego tekstu). To odniesienia projektowe, nie deklaracja
pełnej zgodności gotowej gry. Wskazanie rozmiaru fontu w Unity nie zastępuje
sprawdzenia rzeczywistej wysokości glifów po renderowaniu.

- Tekst 100–200%, z przebudową układu. Obecny zakres w grze to 80–150%; przy
  wdrożeniu trzeba zachować stare wartości ustawień i zaktualizować walidację
  nowych, bez resetowania zapisu. Mobilny interfejs wymaga odrębnego zakresu testów.
- W 1280 × 720 panel szczegółów schodzi do rozwijanej sekcji; nic nie wymusza
  malejącej czcionki. W 2560 × 1440 i 3440 × 1440 utrzymujemy maksymalną szerokość
  czytania, a nadmiar miejsca przeznaczamy na kontekst, nie rozciąganie akapitów.
- W rozdzielczościach PC maksymalnie jeden kierunek przewijania głównej treści.
  Kolumny drugorzędne składają się do szczegółów. Stan lokalnych filtrów zostaje.
- Ikona zawsze ma nazwę lub tekst. Kolor ma dodatkowy znak i słowo: np. trójkąt
  oraz „Brakuje Energy”. Czerwony służy błędom lub destrukcyjnej konsekwencji.
- Tab przechodzi logicznie, Enter/Spacja aktywują, Escape zamyka warstwę szczegółów.
  Fokus wraca do wywołującego elementu. Globalne skróty nie działają podczas edycji
  pól i potwierdzania resetu. Skrót Max dotyczy tylko wybranego procesu.
- Komunikaty nie przesuwają wierszy podczas kliknięcia. Powiadomienia grupujemy,
  a istotne wydarzenia można odnaleźć po ich zniknięciu.
- „Zapisano” pokazujemy po zakończonym zapisie. Stan „zapis w toku” i błąd mają
  osobne etykiety. Te oznaczenia muszą pochodzić z warstwy zapisu.

Przeliczone kontrasty wybranych par: tekst/panel **15,50:1**, tekst pomocniczy/panel
**8,36:1**, tekst pomocniczy/zaznaczenie **7,29:1**, ciemny tekst/turkusowa akcja
**11,92:1**, granica kontrolki/panel **4,86:1**. Wybrane pary motywu Świt także
przekraczają 4,5:1. Log: `Logs/UIPlanning/contrast.json`. Należy później sprawdzić
wszystkie stany komponentów i obrazy tła; to nie jest pełny audyt dostępności.

## 8. Plan wdrożenia

| Etap | Konkretne zadanie | Warunek zakończenia |
|---|---|---|
| A. Podstawa | Tokeny, fonty, wspólne wiersze/przyciski, szkielet 6 sekcji | Widoki i ustawienia skali dostępne; żadna dotychczasowa funkcja nie znika |
| B. Produkcja | Lista, filtry, koszt pakietu, panel realnej wydajności, rezerwy | Zakup i diagnoza ograniczenia bez przechodzenia przez kilka stron |
| C. Rozwój | Resety, mapy/listy węzłów, profile i warunek finału | UI używa rzeczywistych nagród i kontraktu retencji z Core |
| D. Kosmos i automaty | Wspólny stan reguł, budowa, prawa, gospodarki i transfery | Przy każdej zablokowanej czynności widać rzeczywisty powód |
| E. Całość | Menu, dziennik, raport, stany puste/błędy, języki, fokus | Wszystkie 17 obecnych funkcji mają jednoznaczną ścieżkę dostępu |
| F. Walidacja | Testy źródeł, kompilacja, później odrębna ocena graficzna | Osobne dowody dla kompilacji, działania i wyglądu |

Wdrożenie powinno korzystać z istniejącego uGUI/TMP, `Simulation`, `ResetRules`,
offline report i `SaveStore`. Odczyty mogą być przekazywane przez małe modele widoku;
UI nie liczy ponownie balansu. Aktualizacje danych grupujemy, nie przebudowujemy
całego drzewa obiektów przy każdym tyknięciu liczby. Wirtualizacja list tylko tam,
gdzie pomiar wykaże potrzebę. Nie przenosimy projektu do innego frameworka UI.

Pierwszy przekrój do rzeczywistej oceny: **Produkcja + szczegóły procesu + podgląd
Ascension**, z jednym wczesnym i jednym późnym zapisem testowym. Dopiero po jego
sprawdzeniu przenosimy pozostałe ekrany na te same komponenty.

## 9. Kryteria odbioru

W przyszłym teście z użytkownikami: po 5 sekundach gracz ma wskazać zapas i tempo
netto; w 10 sekund znaleźć powód ograniczenia; przed resetem własnymi słowami
powiedzieć, co utraci. To cele do zbadania, jeszcze nie osiągnięte wyniki.

Technicznie sprawdzamy 1280×720, 1920×1080, 2560×1440 i 3440×1440; skalę
100/125/150/200%; długie etykiety PL/EN, wykładniki do zakresu wspieranego przez
Number, zerowe i ujemne tempo netto, brak zasobów, blokadę cooldownu, pusty raport,
długi offline, błąd zapisu, przebudowę po resecie i zmianę wszechświata. Obsługa
klawiaturą musi obejmować także wyjście z modala i przywrócenie fokusu.

Testy regresji mają potwierdzić ten sam bilans zasobów, reset, zapis i tempo
kampanii. Do czasu wykonania testu graficznego status wyglądu i fizycznej obsługi
pozostaje UNVERIFIED. Obowiązuje dotychczasowa praca bez przejmowania pulpitu.

## 10. Dostarczona makieta i dowody tego etapu

Interaktywna koncepcja obejmuje warianty Obserwatorium i Terminal, listę procesów,
przejście do źródła ograniczenia, demonstracyjny zakup, panel resetu, przełączniki
automatyzacji oraz szkice pozostałych sekcji. Podwidoki Badania/Drzewka/Przepływy
są opisami docelowych zachowań, nie kompletnym prototypem tych ekranów. Dane są
przykładowe; demonstracyjne zakupy mają uproszczony koszt i nie symulują gry.
Makieta korzysta z systemowego fontu, jeżeli IBM Plex Sans nie jest dostępny.

Źródło makiety:
`C:/Users/Jakubix/.codex/visualizations/2026/10/10/01a12723-df08-7ab3-8e8c-6d7501d73a98/celestial-observatory.html`.

- **PLANNED:** analiza dziewięciu tytułów, architektura, styl, zachowania i etapy.
- **IMPLEMENTED — tylko makieta:** dwa warianty i lokalne interakcje demonstracyjne.
- **OBSERVED — dokument:** 120 kontroli granic elementów w 6 sekcjach, 2 wariantach,
  5 szerokościach (320–1920) i 2 motywach; bez wykrytych przekroczeń. Zakup,
  diagnoza źródła, modal i przełącznik przeszły kontrole; zero błędów skryptu.
  To kontrole DOM w odizolowanym procesie headless, bez myszy, klawiatury,
  przejmowania okien i zrzutów ekranu. Nie zastępują oględzin ani testu Unity.
- **COMPILED / BUILT:** nie wykonywano nowego builda Unity w zadaniu planistycznym.
- **PENCIL:** serwer zgłosił brak transportu do aplikacji desktop po trzech próbach
  połączenia. Dokument `.pen` nie został w tym etapie zmieniony; plan i makieta są
  zapisane niezależnie. Nie uruchamiano aplikacji w widocznym oknie.

Dowody: `Logs/UIPlanning/prototype-checks.json`, `contrast.json`,
`References/sources.json`. Skrypt kontroli makiety: `Tools/ui-plan-check.mjs`.
Dotychczasowy build 2.1.0, kod gry, balans i zapisy użytkownika pozostają punktem
wyjścia do przyszłego wdrożenia.
