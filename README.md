# CATA — Cellular Automata Toolkit

Framework C# per simulazione, visualizzazione e sperimentazione di automi cellulari.
Sviluppato come supporto alla tesi triennale.

## Requisiti

- .NET 8 SDK (`dotnet --version`)
- Terminale con supporto ANSI (per LiveDemo)

## Build

```bash
cd code/CATA
dotnet build
```

Per build con output pulito:

```bash
dotnet build -c Release
```

## Test

```bash
dotnet test
```

---

## Programmi

### LiveDemo — simulazione in tempo reale nel terminale

Visualizza l'evoluzione dell'automa in tempo reale direttamente nel terminale.

```bash
dotnet run --project Programs/LiveDemo -- [opzioni]
```

**Modalità disponibili:**

| Modalità | Descrizione |
|----------|-------------|
| `--mode 1d` | CA 1D Wolfram, una riga per generazione (scrolling) |
| `--mode 2d` | CA 2D in-place (Game of Life di default) |
| `--mode density` | CA 2D con vicinato adattivo alla densità locale |
| `--mode meta2d` | CA gerarchico: A 2D, B 2D; visualizzati affiancati |

**Opzioni comuni:**

| Opzione | Default | Descrizione |
|---------|---------|-------------|
| `--rule N` | 30 | Numero regola Wolfram (modalità 1d/2d con --meta) |
| `--meta` | off | Abilita meta-regola gerarchica (B=CA 1D) |
| `--cells N` | auto | Larghezza griglia 1D (default = larghezza terminale) |
| `--size N` | auto | Lato griglia 2D (default = altezza terminale) |
| `--fps N` | 10 | Passi al secondo |

**Tasti durante l'esecuzione:**

| Tasto | Effetto |
|-------|---------|
| `R` | Randomizza la griglia |
| `+` / `→` | Regola successiva (in modalità meta/meta2d) |
| `-` / `←` | Regola precedente (in modalità meta/meta2d) |
| `Ctrl+C` | Esci |

**Esempi:**

```bash
# CA 1D Wolfram regola 30
dotnet run --project Programs/LiveDemo -- --mode 1d --rule 30

# CA 1D con meta-regola gerarchica (B=CA con regola 110)
dotnet run --project Programs/LiveDemo -- --mode 1d --meta --rule 110

# Game of Life 2D
dotnet run --project Programs/LiveDemo -- --mode 2d

# Game of Life 2D con meta-regola gerarchica
dotnet run --project Programs/LiveDemo -- --mode 2d --meta --rule 30

# CA 2D con vicinato adattivo alla densità
dotnet run --project Programs/LiveDemo -- --mode density

# CA gerarchico 2D (A e B entrambi griglie 2D)
dotnet run --project Programs/LiveDemo -- --mode meta2d --size 40
```

---

### GenerateFigures — genera le figure per la tesi

Produce tutte le immagini PNG utilizzate nella tesi.

```bash
dotnet run --project Programs/GenerateFigures -- [opzioni]
```

**Opzioni:**

| Opzione | Default | Descrizione |
|---------|---------|-------------|
| `--output <dir>` | `results` | Cartella di output |
| `--seed N` | 42 | Seme per la generazione casuale |

**Figure prodotte:**

| File | Contenuto |
|------|-----------|
| `rule30.png` | CA elementare Wolfram regola 30 |
| `rule110.png` | CA elementare Wolfram regola 110 |
| `meta89.png` | Meta-regola gerarchica con Wolfram 89 |
| `meta110.png` | Meta-regola gerarchica con Wolfram 110 |
| `meta30.png` | Meta-regola gerarchica con Wolfram 30 (caotico) |
| `meta73.png` | Meta-regola gerarchica con Wolfram 73 (complesso) |
| `meta2d_30.png` | Meta-regola 2D; A=50×50, B guidato da Wolfram 30 |
| `density_adapt.png` | Meta-regola con vicinato adattivo alla densità |
| `life.png` | Game of Life dopo 26 passi |

**Rigenerare gli asset della tesi:**

```bash
dotnet run --project Programs/GenerateFigures -- --output ../../assets --seed 42
```

---

### WolframDemo — immagine CA 1D

```bash
dotnet run --project Programs/WolframDemo -- --rule 30 --steps 300 --cells 300
```

Output: `results/wolfram_30.png`

---

### MetaRules — immagine meta-regola 1D

```bash
dotnet run --project Programs/MetaRules -- --rule 89 --steps 200
```

Output: `results/meta89.png`

---

### Plot2D — immagine Game of Life

```bash
dotnet run --project Programs/Plot2D -- --steps 26 --size 100
```

Output: `results/gol.png`

---

## Struttura del codice

```
CATA/
├── Simulation/          # Libreria core
│   └── src/
│       ├── Container/   # Array<T>, Grid2D<T>, BoundaryCondition
│       ├── Rule/        # WolframRule, TotalisticRule, MetaRule, Model1DRule, Model2DRule,
│       │                # DensityNeighborhoodMetaRule, …
│       └── Model.cs     # Loop di simulazione
├── Visualization/       # ChartHelper (PNG heatmap via SkiaSharp)
├── Tests/               # Test unitari (xUnit)
└── Programs/
    ├── LiveDemo/        # Visualizzatore in tempo reale
    ├── GenerateFigures/ # Batch figura tesi
    ├── WolframDemo/     # Singola figura 1D
    ├── MetaRules/       # Singola figura meta-regola
    └── Plot2D/          # Singola figura 2D
```

## Condizioni al contorno

`Grid2D<T>` e `Array<T>` supportano tre modalità via `BoundaryCondition`:

| Valore | Comportamento |
|--------|---------------|
| `Fixed` (default) | Indici fuori range → `DefaultValue` |
| `Periodic` | Wrap toroidale (modulo) |
| `Reflective` | Riflessione speculare al bordo |

```csharp
var grid = new Grid2D<State>(50, 50, defaultState, BoundaryCondition.Periodic);
```
