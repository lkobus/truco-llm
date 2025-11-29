const { createApp } = Vue;

const TrucoViewer = {
    template: `
        <div class="truco-viewer">
            <div class="header">
                <h1>🃏 Truco Game Viewer</h1>
                <div class="match-selector">
                    <input 
                        v-model="matchId" 
                        @keyup.enter="loadMatch"
                        placeholder="Digite o Match ID"
                        class="match-input"
                    />
                    <button @click="loadMatch" class="btn-load">Carregar Partida</button>
                    <button v-if="isPolling" @click="stopPolling" class="btn-stop">Parar</button>
                </div>
            </div>

            <div v-if="error" class="error-message">{{ error }}</div>

            <div v-if="matchState" class="game-container">
                <!-- Placar -->
                <div class="scoreboard">
                    <div class="team-score team-a">
                        <h3>Time A</h3>
                        <div class="score-display">
                            <span class="match-score">{{ matchState.matchScoreTeamA }}</span>
                            <span class="turn-score">({{ matchState.turnScoreTeamA }})</span>
                        </div>
                    </div>
                    <div class="reward-display">
                        <span>Recompensa: {{ matchState.currentReward }}</span>
                        <span class="state-badge">{{ matchState.state }}</span>
                    </div>
                    <div class="team-score team-b">
                        <h3>Time B</h3>
                        <div class="score-display">
                            <span class="match-score">{{ matchState.matchScoreTeamB }}</span>
                            <span class="turn-score">({{ matchState.turnScoreTeamB }})</span>
                        </div>
                    </div>
                </div>

                <!-- Mesa de jogo -->
                <div class="game-table">
                    <!-- Player 0 (Topo) -->
                    <div class="player-position top">
                        <player-hand 
                            v-if="getPlayer(0)"
                            :player="getPlayer(0)"
                            :is-current="isCurrentPlayer(0)"
                        />
                    </div>

                    <!-- Player 3 (Esquerda) -->
                    <div class="player-position left">
                        <player-hand 
                            v-if="getPlayer(3)"
                            :player="getPlayer(3)"
                            :is-current="isCurrentPlayer(3)"
                        />
                    </div>

                    <!-- Centro: Manilha e Cartas Jogadas -->
                    <div class="center-area">
                        <div class="manilha-container">
                            <div class="manilha-label">Manilha</div>
                            <card-component 
                                v-if="matchState.manilha"
                                :card="matchState.manilha"
                                :size="'medium'"
                            />
                        </div>

                        <div class="played-cards">
                            <div 
                                v-for="played in matchState.playedCards" 
                                :key="played.playerId"
                                :class="['played-card', getPlayerPositionClass(played.playerId)]"
                            >
                                <card-component 
                                    :card="played.card"
                                    :size="'medium'"
                                />
                            </div>
                        </div>
                    </div>

                    <!-- Player 1 (Direita) -->
                    <div class="player-position right">
                        <player-hand 
                            v-if="getPlayer(1)"
                            :player="getPlayer(1)"
                            :is-current="isCurrentPlayer(1)"
                        />
                    </div>

                    <!-- Player 2 (Baixo) -->
                    <div class="player-position bottom">
                        <player-hand 
                            v-if="getPlayer(2)"
                            :player="getPlayer(2)"
                            :is-current="isCurrentPlayer(2)"
                        />
                    </div>
                </div>

                <!-- Info da rodada -->
                <div class="round-info">
                    <p>Turno: {{ matchState.currentTurn }}</p>
                    <p v-if="matchState.currentPlayerId">
                        Vez do jogador: {{ getPlayerName(matchState.currentPlayerId) }}
                    </p>
                </div>

                <!-- Comentários -->
                <div v-if="matchState.comments && matchState.comments.length > 0" class="comments-panel">
                    <h3>💬 Comentários</h3>
                    <div class="comments-list">
                        <div 
                            v-for="(comment, index) in matchState.comments.slice(-5)" 
                            :key="index"
                            class="comment-item"
                        >
                            {{ comment }}
                        </div>
                    </div>
                </div>
            </div>

            <div v-else-if="!error" class="welcome-message">
                <h2>Bem-vindo ao Truco Viewer!</h2>
                <p>Digite o ID de uma partida para visualizar o jogo em tempo real.</p>
            </div>
        </div>
    `,
    data() {
        return {
            matchId: '',
            matchState: null,
            error: null,
            isPolling: false,
            pollingInterval: null,
            apiBaseUrl: 'http://localhost:5002/api'
        };
    },
    methods: {
        async loadMatch() {
            if (!this.matchId.trim()) {
                this.error = 'Por favor, digite um Match ID válido';
                return;
            }

            this.error = null;
            
            try {
                const response = await axios.get(
                    `${this.apiBaseUrl}/matches/${this.matchId}/state`
                );

                if (response.data.success) {
                    this.matchState = response.data.data;
                    console.log('Match state loaded:', this.matchState);
                    console.log('Players:', this.matchState.players);
                    console.log('Played cards:', this.matchState.playedCards);
                    this.startPolling();
                } else {
                    this.error = response.data.message;
                }
            } catch (err) {
                this.error = err.response?.data?.message || 'Erro ao carregar a partida';
                console.error('Erro:', err);
            }
        },
        async refreshMatchState() {
            if (!this.matchId) return;

            try {
                const response = await axios.get(
                    `${this.apiBaseUrl}/matches/${this.matchId}/state`
                );

                if (response.data.success) {
                    const newState = response.data.data;
                    
                    // Se a mesa foi limpa (nenhuma carta jogada), detectamos
                    if (this.matchState && 
                        this.matchState.playedCards.length > 0 && 
                        newState.playedCards.length === 0) {
                        console.log('Mesa limpa!');
                    }
                    
                    this.matchState = newState;
                } else {
                    this.error = response.data.message;
                    this.stopPolling();
                }
            } catch (err) {
                if (err.response?.status === 404) {
                    this.error = 'Partida não encontrada ou foi encerrada';
                    this.stopPolling();
                }
                console.error('Erro ao atualizar:', err);
            }
        },
        startPolling() {
            if (this.isPolling) return;
            
            this.isPolling = true;
            this.pollingInterval = setInterval(() => {
                this.refreshMatchState();
            }, 1000); // Atualiza a cada 1 segundo
        },
        stopPolling() {
            this.isPolling = false;
            if (this.pollingInterval) {
                clearInterval(this.pollingInterval);
                this.pollingInterval = null;
            }
        },
        getPlayer(position) {
            return this.matchState?.players.find(p => p.position === position);
        },
        isCurrentPlayer(position) {
            const player = this.getPlayer(position);
            return player && player.id === this.matchState?.currentPlayerId;
        },
        getPlayerName(playerId) {
            const player = this.matchState?.players.find(p => p.id === playerId);
            return player?.name || `Player ${playerId}`;
        },
        getPlayerPositionClass(playerId) {
            const player = this.matchState?.players.find(p => p.id === playerId);
            if (!player) return '';
            
            const positions = ['top', 'right', 'bottom', 'left'];
            return positions[player.position] || '';
        }
    },
    beforeUnmount() {
        this.stopPolling();
    }
};

const PlayerHand = {
    props: {
        player: {
            type: Object,
            required: true
        },
        isCurrent: {
            type: Boolean,
            default: false
        }
    },
    computed: {
        teamClass() {
            // Jogadores 0 e 2 são Time A, jogadores 1 e 3 são Time B
            return this.player.position === 0 || this.player.position === 2 ? 'team-a' : 'team-b';
        }
    },
    template: `
        <div :class="['player-hand', teamClass, { 'current-player': isCurrent }]">
            <div class="player-name">
                {{ player.name }}
                <span v-if="isCurrent" class="current-indicator">👈</span>
            </div>
            <div class="cards">
                <card-component 
                    v-for="(card, index) in player.hand" 
                    :key="index"
                    :card="card"
                    :size="'small'"
                />
            </div>
        </div>
    `,
    mounted() {
        console.log('PlayerHand montado:', this.player);
        console.log('Número de cartas na mão:', this.player.hand?.length || 0);
    }
};

const CardComponent = {
    props: {
        card: {
            type: Object,
            required: true
        },
        size: {
            type: String,
            default: 'small' // small, medium, large
        }
    },
    computed: {
        cardImage() {
            console.log('CardComponent - card:', this.card);
            
            if (this.card.hide) {
                return 'assets/card_back.png';
            }
            
            // Mapear os nomes das cartas para os arquivos
            const suitMap = {
                'Hearts': 'hearts',
                'Diamonds': 'diamonds',
                'Clubs': 'clubs',
                'Spades': 'spades'
            };
            
            // TrucoDeck usa: rank 1=4, 2=5, 3=6, 4=7, 5=Q, 6=J, 7=K, 8=A, 9=2, 10=3
            const rankMap = {
                1: '04',  // 4
                2: '05',  // 5
                3: '06',  // 6
                4: '07',  // 7
                5: 'Q',   // Q
                6: 'J',   // J
                7: 'K',   // K
                8: 'A',   // A
                9: '02',  // 2
                10: '03'  // 3
            };
            
            const suit = suitMap[this.card.suit] || 'hearts';
            const rank = rankMap[this.card.rank];
            
            if (!rank) {
                console.error('Rank inválido:', this.card.rank, 'Carta:', this.card);
                return 'assets/card_back.png';
            }
            
            const imagePath = `assets/card_${suit}_${rank}.png`;
            console.log('Caminho da imagem:', imagePath);
            return imagePath;
        }
    },
    template: `
        <div :class="['card', size]">
            <img :src="cardImage" :alt="card.name" @error="handleImageError" />
        </div>
    `,
    methods: {
        handleImageError(e) {
            console.error('Erro ao carregar imagem:', this.cardImage);
            console.error('URL tentada:', e.target.src);
            e.target.src = 'assets/card_back.png';
        }
    },
    mounted() {
        console.log('CardComponent montado com carta:', this.card);
    }
};

createApp({
    components: {
        'truco-viewer': TrucoViewer,
        'player-hand': PlayerHand,
        'card-component': CardComponent
    }
}).mount('#app');
