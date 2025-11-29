# 🃏 Guia de Referência das Cartas

## Nomenclatura dos Arquivos

Os arquivos de imagens seguem o padrão: `card_{suit}_{rank}.png`

### Naipes (Suits)
- **hearts** = Copas (♥️)
- **diamonds** = Ouros (♦️)
- **clubs** = Paus (♣️)
- **spades** = Espadas (♠️)

### Ranks

**IMPORTANTE: O sistema TrucoDeck usa o seguinte mapeamento:**

```
Rank 1  = 4 (Quatro)
Rank 2  = 5 (Cinco)
Rank 3  = 6 (Seis)
Rank 4  = 7 (Sete)
Rank 5  = Q (Dama)
Rank 6  = J (Valete)
Rank 7  = K (Rei)
Rank 8  = A (Ás)
Rank 9  = 2 (Dois)
Rank 10 = 3 (Três)
```

### Arquivos de Imagem
- **04** = 4 (card_hearts_04.png)
- **05** = 5 (card_hearts_05.png)
- **06** = 6 (card_hearts_06.png)
- **07** = 7 (card_hearts_07.png)
- **Q** = Dama (card_hearts_Q.png)
- **J** = Valete (card_hearts_J.png)
- **K** = Rei (card_hearts_K.png)
- **A** = Ás (card_hearts_A.png)
- **02** = 2 (card_hearts_02.png)
- **03** = 3 (card_hearts_03.png)

## Mapeamento Completo

### Copas (Hearts)
```
Rank 1  → card_hearts_04.png  (4)
Rank 2  → card_hearts_05.png  (5)
Rank 3  → card_hearts_06.png  (6)
Rank 4  → card_hearts_07.png  (7)
Rank 5  → card_hearts_Q.png   (Dama)
Rank 6  → card_hearts_J.png   (Valete)
Rank 7  → card_hearts_K.png   (Rei)
Rank 8  → card_hearts_A.png   (Ás)
Rank 9  → card_hearts_02.png  (2)
Rank 10 → card_hearts_03.png  (3)
```

### Ouros (Diamonds)
```
Rank 1  → card_diamonds_04.png  (4)
Rank 2  → card_diamonds_05.png  (5)
Rank 3  → card_diamonds_06.png  (6)
Rank 4  → card_diamonds_07.png  (7)
Rank 5  → card_diamonds_Q.png   (Dama)
Rank 6  → card_diamonds_J.png   (Valete)
Rank 7  → card_diamonds_K.png   (Rei)
Rank 8  → card_diamonds_A.png   (Ás)
Rank 9  → card_diamonds_02.png  (2)
Rank 10 → card_diamonds_03.png  (3)
```

### Paus (Clubs)
```
Rank 1  → card_clubs_04.png  (4)
Rank 2  → card_clubs_05.png  (5)
Rank 3  → card_clubs_06.png  (6)
Rank 4  → card_clubs_07.png  (7)
Rank 5  → card_clubs_Q.png   (Dama)
Rank 6  → card_clubs_J.png   (Valete)
Rank 7  → card_clubs_K.png   (Rei)
Rank 8  → card_clubs_A.png   (Ás)
Rank 9  → card_clubs_02.png  (2)
Rank 10 → card_clubs_03.png  (3)
```

### Espadas (Spades)
```
Rank 1  → card_spades_04.png  (4)
Rank 2  → card_spades_05.png  (5)
Rank 3  → card_spades_06.png  (6)
Rank 4  → card_spades_07.png  (7)
Rank 5  → card_spades_Q.png   (Dama)
Rank 6  → card_spades_J.png   (Valete)
Rank 7  → card_spades_K.png   (Rei)
Rank 8  → card_spades_A.png   (Ás)
Rank 9  → card_spades_02.png  (2)
Rank 10 → card_spades_03.png  (3)
```

### Especiais
```
card_back.png        → Verso da carta (Hide = true)
card_empty.png       → Carta vazia
card_joker_black.png → Coringa preto
card_joker_red.png   → Coringa vermelho
```

## Uso no Código

### JavaScript
```javascript
const suitMap = {
    'Hearts': 'hearts',
    'Diamonds': 'diamonds',
    'Clubs': 'clubs',
    'Spades': 'spades'
};

// TrucoDeck usa este mapeamento de ranks
const rankMap = {
    1: '04',  // 4
    2: '05',  // 5
    3: '06',  // 6
    4: '07',  // 7
    5: 'Q',   // Dama
    6: 'J',   // Valete
    7: 'K',   // Rei
    8: 'A',   // Ás
    9: '02',  // 2
    10: '03'  // 3
};

// Gerar nome do arquivo
const suit = suitMap[card.suit];
const rank = rankMap[card.rank];
const filename = `card_${suit}_${rank}.png`;
```

## Exemplos

### Carta do Backend
```json
{
  "name": "4",
  "suit": "Hearts",
  "rank": 1,
  "suitRank": 1,
  "hide": false
}
```

**Resultado**: `card_hearts_04.png`

---

### Carta do Backend
```json
{
  "name": "7",
  "suit": "Spades",
  "rank": 4,
  "suitRank": 4,
  "hide": false
}
```

**Resultado**: `card_spades_07.png`

---

### Carta do Backend
```json
{
  "name": "Q",
  "suit": "Diamonds",
  "rank": 5,
  "suitRank": 2,
  "hide": false
}
```

**Resultado**: `card_diamonds_Q.png`

---

### Carta Oculta
```json
{
  "name": "A",
  "suit": "Clubs",
  "rank": 8,
  "suitRank": 3,
  "hide": true
}
```

**Resultado**: `card_back.png` (ignora suit/rank quando hide=true)

## SuitRank Reference

O `suitRank` determina a força do naipe nas manilhas:

```
1 = Hearts (Copas)     - Mais fraco
2 = Diamonds (Ouros)
3 = Clubs (Paus)
4 = Spades (Espadas)   - Mais forte
```

## Truco Card Ranks

No Truco, nem todas as cartas são usadas. As cartas 8, 9, 10 (numéricas) não existem no baralho de Truco tradicional.

### Cartas Usadas no Truco
- **Rank 1-7**: Ás, 2, 3, 4, 5, 6, 7
- **Rank 10**: Valete (Q)
- **Rank 11**: Dama (K)
- **Rank 12**: Rei (A)

### Cartas NÃO Usadas
- Rank 8, 9 (cartas 08, 09, 10 numéricas)
- Coringas

## Debugging

Se uma carta não aparece, verifique:

1. **Nome do arquivo existe?**
   ```
   ls assets/card_*.png
   ```

2. **Mapeamento correto?**
   - Suit: Hearts/Diamonds/Clubs/Spades
   - Rank: 1-7, 10-12

3. **Console do navegador**
   ```
   F12 → Console → Ver erros de imagem
   ```

4. **Fallback**
   - Se imagem falhar, mostra `card_back.png`
   - Verifique o console para mensagem de erro

## Asset Requirements

### Obrigatórias
- ✅ `card_back.png` - Usada para cartas ocultas

### Recomendadas (Truco)
- ✅ Copas: A, 02-07, J, Q, K
- ✅ Ouros: A, 02-07, J, Q, K
- ✅ Paus: A, 02-07, J, Q, K
- ✅ Espadas: A, 02-07, J, Q, K

### Opcionais
- card_empty.png
- card_joker_black.png
- card_joker_red.png
- Cartas 08, 09, 10 (não usadas no Truco tradicional)

## Nomenclatura no Backend vs Assets

| Backend | Assets | Carta |
|---------|--------|-------|
| suit: "Hearts" | hearts | Copas |
| suit: "Diamonds" | diamonds | Ouros |
| suit: "Clubs" | clubs | Paus |
| suit: "Spades" | spades | Espadas |
| rank: 1 | 04 | 4 |
| rank: 2 | 05 | 5 |
| rank: 3 | 06 | 6 |
| rank: 4 | 07 | 7 |
| rank: 5 | Q | Dama |
| rank: 6 | J | Valete |
| rank: 7 | K | Rei |
| rank: 8 | A | Ás |
| rank: 9 | 02 | 2 |
| rank: 10 | 03 | 3 |
| hide: true | card_back.png | Verso |

---

## Quick Reference

```
Backend → Asset

Hearts + rank 1  → card_hearts_04.png (4)
Hearts + rank 4  → card_hearts_07.png (7)
Hearts + rank 8  → card_hearts_A.png  (Ás)
Spades + rank 5  → card_spades_Q.png  (Dama)
Diamonds + rank 6 → card_diamonds_J.png (Valete)
Clubs + rank 7   → card_clubs_K.png   (Rei)
Any + hide:true  → card_back.png
```
