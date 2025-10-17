using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PrecisionDriverTester
{
    public partial class Form1 : Form
    {
        private PrecisionDriver _driver;
        private System.Windows.Forms.Timer _statsTimer;
        // RIMOSSO: _inputTimer che causava la race condition
        private Thread _pollingThread;
        private volatile bool _pollingActive = false;

        // Contatori per monitorare l'attività del polling
        private long _pollingCallsCount = 0;
        private long _lastPollingCountForStats = 0;
        private DateTime _lastStatsTime = DateTime.Now;
        private bool _pollingStuck = false;

        // Controls
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnEnableIntercept;
        private Button btnDisableIntercept;
        private Button btnClearInputs;
        private Button btnApplyPolicy;
        private Button btnToggleGlobalPolicy;

        private CheckBox chkBlockKeyboard;
        private CheckBox chkBlockMouse;
        private CheckBox chkBlockMouseButtons;
        private CheckBox chkBlockMouseMovement;

        private Label lblConnectionStatus;
        private Label lblTotalInputs;
        private Label lblBlockedInputs;
        private Label lblKeyboardInputs;
        private Label lblMouseInputs;
        private Label lblInterceptStatus;
        private Label lblPollingStatus;
        private Label lblPollingCalls;

        Label lblEventsReadTitle = new Label();
        Label lblEventsRead = new Label();  // ← NUOVA LABEL CRITICA
        Label lblCaptureRate = new Label();  // ← PERCENTUALE DI CAPTURE

        private ListBox lstInputs;
        private TextBox txtSpecificKeys;

        private GroupBox grpConnection;
        private GroupBox grpIntercept;
        private GroupBox grpBlockPolicy;
        private GroupBox grpStatistics;
        private GroupBox grpInputs;
        private GroupBox grpPollingMonitor;

        private long _lastKnownTotalInputs = 0;
        private long _totalEventsReadByThread = 0; // NUOVO: contatore gestito SOLO dal thread
        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            _driver = new PrecisionDriver();

            // *** VERIFICA CRITICA ***
            int size = Marshal.SizeOf<InterceptedInput>();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine($"sizeof(InterceptedInput) = {size} bytes");
            Console.WriteLine($"ATTESO: 26 bytes");

            if (size == 26)
            {
                Console.WriteLine("✓✓✓ DIMENSIONE CORRETTA! ✓✓✓");
            }
            else
            {
                Console.WriteLine($"✗✗✗ ERRORE: Dimensione errata! Atteso 26, ottenuto {size} ✗✗✗");
                MessageBox.Show($"ERRORE CRITICO: La struttura InterceptedInput è {size} bytes invece di 26!\n\n" +
                               "Il programma NON funzionerà correttamente!",
                               "Errore Struttura", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Console.WriteLine("═══════════════════════════════════════════");

            // Setup timer SOLO per le statistiche
            _statsTimer = new System.Windows.Forms.Timer();
            _statsTimer.Interval = 1000;
            _statsTimer.Tick += StatsTimer_Tick;
        }

        private void InitializeCustomComponents()
        {
            this.Text = "PrecisionDrv Tester - Race Condition Fixed";
            this.Size = new Size(900, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.LightGray;

            // Connection Group
            grpConnection = new GroupBox();
            grpConnection.Text = "Connessione Driver";
            grpConnection.Location = new Point(10, 10);
            grpConnection.Size = new Size(860, 80);

            btnConnect = new Button();
            btnConnect.Text = "Connetti";
            btnConnect.Location = new Point(10, 25);
            btnConnect.Size = new Size(100, 30);
            btnConnect.Click += BtnConnect_Click;

            btnDisconnect = new Button();
            btnDisconnect.Text = "Disconnetti";
            btnDisconnect.Location = new Point(120, 25);
            btnDisconnect.Size = new Size(100, 30);
            btnDisconnect.Click += BtnDisconnect_Click;
            btnDisconnect.Enabled = false;

            lblConnectionStatus = new Label();
            lblConnectionStatus.Text = "Status: Disconnesso";
            lblConnectionStatus.Location = new Point(240, 30);
            lblConnectionStatus.Size = new Size(200, 20);
            lblConnectionStatus.ForeColor = Color.Red;
            lblConnectionStatus.Font = new Font(lblConnectionStatus.Font, FontStyle.Bold);

            grpConnection.Controls.AddRange(new Control[] { btnConnect, btnDisconnect, lblConnectionStatus });

            // Intercept Group
            grpIntercept = new GroupBox();
            grpIntercept.Text = "Controllo Intercettazione";
            grpIntercept.Location = new Point(10, 100);
            grpIntercept.Size = new Size(860, 80);

            btnEnableIntercept = new Button();
            btnEnableIntercept.Text = "Abilita Intercettazione";
            btnEnableIntercept.Location = new Point(10, 25);
            btnEnableIntercept.Size = new Size(150, 30);
            btnEnableIntercept.Click += BtnEnableIntercept_Click;
            btnEnableIntercept.Enabled = false;

            btnDisableIntercept = new Button();
            btnDisableIntercept.Text = "Disabilita Intercettazione";
            btnDisableIntercept.Location = new Point(170, 25);
            btnDisconnect.Size = new Size(150, 30);
            btnDisableIntercept.Click += BtnDisableIntercept_Click;
            btnDisableIntercept.Enabled = false;

            btnToggleGlobalPolicy = new Button();
            btnToggleGlobalPolicy.Text = "ON/OFF Test Globale";
            btnToggleGlobalPolicy.Location = new Point(330, 25);
            btnToggleGlobalPolicy.Size = new Size(150, 30);
            btnToggleGlobalPolicy.Click += BtnToggleGlobalPolicy_Click;
            btnToggleGlobalPolicy.Enabled = false;
            btnToggleGlobalPolicy.BackColor = Color.LightBlue;

            lblInterceptStatus = new Label();
            lblInterceptStatus.Text = "Intercettazione: Disabilitata";
            lblInterceptStatus.Location = new Point(500, 30);
            lblInterceptStatus.Size = new Size(200, 20);
            lblInterceptStatus.Font = new Font(lblInterceptStatus.Font, FontStyle.Bold);

            grpIntercept.Controls.AddRange(new Control[] {
                btnEnableIntercept, btnDisableIntercept, btnToggleGlobalPolicy, lblInterceptStatus
            });

            // Polling Monitor Group
            grpPollingMonitor = new GroupBox();
            grpPollingMonitor.Text = "Monitoraggio Polling (Fix Race Condition)";
            grpPollingMonitor.Location = new Point(10, 190);
            grpPollingMonitor.Size = new Size(860, 80);
            grpPollingMonitor.BackColor = Color.LightYellow;

            lblPollingStatus = new Label();
            lblPollingStatus.Text = "Polling: FERMO";
            lblPollingStatus.Location = new Point(10, 25);
            lblPollingStatus.Size = new Size(250, 20);
            lblPollingStatus.ForeColor = Color.Red;
            lblPollingStatus.Font = new Font(lblPollingStatus.Font, FontStyle.Bold);

            lblPollingCalls = new Label();
            lblPollingCalls.Text = "Chiamate Polling: 0";
            lblPollingCalls.Location = new Point(10, 45);
            lblPollingCalls.Size = new Size(300, 20);

            Label lblPollingInfo = new Label();
            lblPollingInfo.Text = "THREAD UNICO - Nessuna race condition tra timer e thread!";
            lblPollingInfo.Location = new Point(320, 30);
            lblPollingInfo.Size = new Size(530, 20);
            lblPollingInfo.ForeColor = Color.DarkGreen;
            lblPollingInfo.Font = new Font(lblPollingInfo.Font, FontStyle.Bold);

            grpPollingMonitor.Controls.AddRange(new Control[] {
                lblPollingStatus, lblPollingCalls, lblPollingInfo
            });

            // Block Policy Group
            grpBlockPolicy = new GroupBox();
            grpBlockPolicy.Text = "Policy di Blocco";
            grpBlockPolicy.Location = new Point(10, 280);
            grpBlockPolicy.Size = new Size(420, 180);

            chkBlockKeyboard = new CheckBox();
            chkBlockKeyboard.Text = "Blocca Tastiera";
            chkBlockKeyboard.Location = new Point(10, 25);
            chkBlockKeyboard.Size = new Size(150, 20);

            chkBlockMouse = new CheckBox();
            chkBlockMouse.Text = "Blocca Mouse";
            chkBlockMouse.Location = new Point(10, 50);
            chkBlockMouse.Size = new Size(150, 20);

            chkBlockMouseButtons = new CheckBox();
            chkBlockMouseButtons.Text = "Blocca Bottoni Mouse";
            chkBlockMouseButtons.Location = new Point(10, 75);
            chkBlockMouseButtons.Size = new Size(150, 20);

            chkBlockMouseMovement = new CheckBox();
            chkBlockMouseMovement.Text = "Blocca Movimento Mouse";
            chkBlockMouseMovement.Location = new Point(10, 100);
            chkBlockMouseMovement.Size = new Size(180, 20);

            Label lblSpecificKeys = new Label();
            lblSpecificKeys.Text = "Scan codes (corretti): 16=Q, 17=W, 18=E, 30=A, 31=S, 29=CTRL";
            lblSpecificKeys.Location = new Point(10, 125);
            lblSpecificKeys.Size = new Size(400, 20);

            txtSpecificKeys = new TextBox();
            txtSpecificKeys.Location = new Point(10, 145);
            txtSpecificKeys.Size = new Size(300, 20);
            txtSpecificKeys.Text = "16,17,18,30,31,29"; // Scan codes corretti

            btnApplyPolicy = new Button();
            btnApplyPolicy.Text = "Applica Policy";
            btnApplyPolicy.Location = new Point(320, 142);
            btnApplyPolicy.Size = new Size(90, 25);
            btnApplyPolicy.Click += BtnApplyPolicy_Click;
            btnApplyPolicy.Enabled = false;

            grpBlockPolicy.Controls.AddRange(new Control[] {
                chkBlockKeyboard, chkBlockMouse, chkBlockMouseButtons, chkBlockMouseMovement,
                lblSpecificKeys, txtSpecificKeys, btnApplyPolicy
            });

            // Statistics Group
            grpStatistics = new GroupBox();
            grpStatistics.Text = "Statistiche Driver";
            grpStatistics.Location = new Point(450, 280);
            grpStatistics.Size = new Size(420, 180);

            lblTotalInputs = new Label();
            lblTotalInputs.Text = "Input Totali: 0";
            lblTotalInputs.Location = new Point(10, 25);
            lblTotalInputs.Size = new Size(150, 20);

            lblBlockedInputs = new Label();
            lblBlockedInputs.Text = "Input Bloccati: 0";
            lblBlockedInputs.Location = new Point(10, 50);
            lblBlockedInputs.Size = new Size(150, 20);
            lblBlockedInputs.ForeColor = Color.Red;
            lblBlockedInputs.Font = new Font(lblBlockedInputs.Font, FontStyle.Bold);

            lblKeyboardInputs = new Label();
            lblKeyboardInputs.Text = "Input Tastiera: 0";
            lblKeyboardInputs.Location = new Point(10, 75);
            lblKeyboardInputs.Size = new Size(150, 20);

            lblMouseInputs = new Label();
            lblMouseInputs.Text = "Input Mouse: 0";
            lblMouseInputs.Location = new Point(10, 100);
            lblMouseInputs.Size = new Size(150, 20);



            lblEventsReadTitle.Text = "─────────────────────";
            lblEventsReadTitle.Location = new Point(10, 125);
            lblEventsReadTitle.Size = new Size(150, 20);
            lblEventsReadTitle.ForeColor = Color.Gray;

            
            lblEventsRead.Name = "lblEventsRead";
            lblEventsRead.Text = "Eventi Letti C#: 0";
            lblEventsRead.Location = new Point(10, 145);
            lblEventsRead.Size = new Size(200, 20);
            lblEventsRead.ForeColor = Color.Blue;
            lblEventsRead.Font = new Font(lblEventsRead.Font, FontStyle.Bold);

           
            lblCaptureRate.Name = "lblCaptureRate";
            lblCaptureRate.Text = "Capture Rate: N/A";
            lblCaptureRate.Location = new Point(10, 165);
            lblCaptureRate.Size = new Size(200, 20);
            lblCaptureRate.ForeColor = Color.Green;
            lblCaptureRate.Font = new Font(lblCaptureRate.Font, FontStyle.Bold);


            Label lblTestInfo = new Label();
            lblTestInfo.Text = "TEST CORRETTO:\n1. Abilita Intercettazione\n2. Premi ON/OFF Test\n3. Ora OGNI tasto dovrebbe apparire!";
            lblTestInfo.Location = new Point(180, 25);
            lblTestInfo.Size = new Size(230, 100);
            lblTestInfo.ForeColor = Color.DarkBlue;
            lblTestInfo.Font = new Font(lblTestInfo.Font, FontStyle.Regular);

            grpStatistics.Controls.AddRange(new Control[] {
                lblTotalInputs, lblBlockedInputs, lblKeyboardInputs, lblMouseInputs, lblTestInfo
            });

            // Inputs Group
            grpInputs = new GroupBox();
            grpInputs.Text = "Input Intercettati (THREAD UNICO) - Rosso=BLOCCATO, Verde=PASSATO";
            grpInputs.Location = new Point(10, 470);
            grpInputs.Size = new Size(860, 290);

            lstInputs = new ListBox();
            lstInputs.Location = new Point(10, 25);
            lstInputs.Size = new Size(770, 250);
            lstInputs.Font = new Font("Consolas", 9);
            lstInputs.DrawMode = DrawMode.OwnerDrawFixed;
            lstInputs.DrawItem += LstInputs_DrawItem;

            btnClearInputs = new Button();
            btnClearInputs.Text = "Pulisci\nLista";
            btnClearInputs.Location = new Point(790, 25);
            btnClearInputs.Size = new Size(60, 50);
            btnClearInputs.Click += BtnClearInputs_Click;

            grpInputs.Controls.AddRange(new Control[] { lstInputs, btnClearInputs });

            // Aggiungi al gruppo
            grpStatistics.Controls.Add(lblEventsReadTitle);
            grpStatistics.Controls.Add(lblEventsRead);
            grpStatistics.Controls.Add(lblCaptureRate);

            // Add all groups to form
            this.Controls.AddRange(new Control[] {
                grpConnection, grpIntercept, grpPollingMonitor, grpBlockPolicy, grpStatistics, grpInputs
            });
        }

        private void LstInputs_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            string text = lstInputs.Items[e.Index].ToString();
            Color textColor = Color.White;
            Color backgroundColor = Color.LightGray;

            if (text.Contains("BLOCCATO"))
            {
                backgroundColor = Color.DarkRed;
            }
            else if (text.Contains("PASSATO"))
            {
                backgroundColor = Color.DarkGreen;
            }

            using (Brush backgroundBrush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
            }

            using (Brush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(text, e.Font, textBrush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        // ================================
        // FIX: TOGGLE POLICY CORRETTO
        // ================================
        private void BtnToggleGlobalPolicy_Click(object sender, EventArgs e)
        {
            Console.WriteLine("=== TOGGLE POLICY CHIAMATO ===");

            // Il polling deve essere già attivo per testare la policy
            if (!_pollingActive)
            {
                MessageBox.Show("Prima abilita l'intercettazione, poi testa la policy!",
                              "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var policy = new BlockPolicy(true);

            if (btnToggleGlobalPolicy.BackColor == Color.LightBlue)
            {
                Console.WriteLine("ATTIVANDO POLICY DI BLOCCO...");

                // *** SCAN CODES CORRETTI ***
                policy.BlockMouseMovement = 1;
                policy.BlockSpecificKeys[30] = 1; // A (0x1E = 30)
                policy.BlockSpecificKeys[31] = 1; // S (0x1F = 31)  
                policy.BlockSpecificKeys[29] = 1; // CTRL (0x1D = 29) ✓ CORRETTO!
                policy.BlockSpecificKeys[16] = 1; // Q (0x10 = 16)
                policy.BlockSpecificKeys[17] = 1; // W (0x11 = 17)
                policy.BlockSpecificKeys[18] = 1; // E (0x12 = 18)

                // AGGIUNGI LE FRECCE PER TEST COMPLETO
                policy.BlockSpecificKeys[72] = 1; // UP (0x48 = 72)
                policy.BlockSpecificKeys[80] = 1; // DOWN (0x50 = 80)
                policy.BlockSpecificKeys[75] = 1; // LEFT (0x4B = 75)
                policy.BlockSpecificKeys[77] = 1; // RIGHT (0x4D = 77)

                btnToggleGlobalPolicy.BackColor = Color.Orange;
                btnToggleGlobalPolicy.Text = "POLICY ATTIVA";

                Console.WriteLine("Policy attivata: A,S,Q,W,E,CTRL,FRECCE bloccati + movimento mouse");
            }
            else
            {
                Console.WriteLine("DISATTIVANDO POLICY DI BLOCCO...");
                // Policy già inizializzata a zero
                btnToggleGlobalPolicy.BackColor = Color.LightBlue;
                btnToggleGlobalPolicy.Text = "ON/OFF Test Globale";

                Console.WriteLine("Policy disattivata: tutti gli input passano");
            }

            bool policyResult = ApplyGlobalPolicy(policy);
            Console.WriteLine($"Risultato applicazione policy: {policyResult}");
        }

        private bool ApplyGlobalPolicy(BlockPolicy policy)
        {
            bool success = _driver.SetBlockPolicy(policy);
            if (success)
            {
                Console.WriteLine("✓ Policy applicata con successo al driver.");
            }
            else
            {
                Console.WriteLine("✗ ERRORE: Impossibile applicare la policy al driver!");
                MessageBox.Show("Errore nell'applicazione della policy.", "Errore",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return success;
        }

        // ================================
        // GESTIONE CONNESSIONE
        // ================================
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Tentativo di connessione al driver...");

            if (_driver.Connect())
            {
                lblConnectionStatus.Text = "Status: Connesso";
                lblConnectionStatus.ForeColor = Color.Green;
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnEnableIntercept.Enabled = true;
                btnDisableIntercept.Enabled = true;
                btnApplyPolicy.Enabled = true;
                btnToggleGlobalPolicy.Enabled = true;

                _statsTimer.Start();

                Console.WriteLine("✓ Connessione al driver riuscita!");
                MessageBox.Show("Connessione al driver riuscita!", "Successo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Console.WriteLine("✗ Impossibile connettersi al driver!");
                MessageBox.Show("Impossibile connettersi al driver. Assicurati che sia installato e caricato.",
                              "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            _statsTimer.Stop();
            StopPollingThread(); // Helper function per pulizia

            _driver.Disconnect();

            lblConnectionStatus.Text = "Status: Disconnesso";
            lblConnectionStatus.ForeColor = Color.Red;
            lblPollingStatus.Text = "Polling: FERMO";
            lblPollingStatus.ForeColor = Color.Red;
            lblPollingCalls.Text = "Chiamate Polling: 0";
            _pollingCallsCount = 0;

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnEnableIntercept.Enabled = false;
            btnDisableIntercept.Enabled = false;
            btnApplyPolicy.Enabled = false;
            btnToggleGlobalPolicy.Enabled = false;

            Console.WriteLine("Disconnesso dal driver.");
        }

        // ================================
        // GESTIONE INTERCETTAZIONE (FIX)
        // ================================
        private void BtnEnableIntercept_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Abilitazione intercettazione...");

            if (_driver.EnableIntercept())
            {
                lblInterceptStatus.Text = "Intercettazione: Abilitata";
                lblInterceptStatus.ForeColor = Color.Green;

                // *** AVVIA L'UNICO THREAD DI POLLING ***
                StartPollingThread();

                Console.WriteLine("✓ Intercettazione abilitata! Thread unico avviato.");
                MessageBox.Show("Intercettazione abilitata! Thread polling avviato.", "Successo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Console.WriteLine("✗ Errore nell'abilitazione dell'intercettazione.");
                MessageBox.Show("Errore nell'abilitazione dell'intercettazione.", "Errore",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDisableIntercept_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Disabilitazione intercettazione...");

            StopPollingThread();

            if (_driver.DisableIntercept())
            {
                lblInterceptStatus.Text = "Intercettazione: Disabilitata";
                lblInterceptStatus.ForeColor = Color.Red;

                Console.WriteLine("✓ Intercettazione disabilitata! Thread fermato.");
                MessageBox.Show("Intercettazione disabilitata!", "Successo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Console.WriteLine("✗ Errore nella disabilitazione dell'intercettazione.");
                MessageBox.Show("Errore nella disabilitazione dell'intercettazione.", "Errore",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ================================
        // GESTIONE THREAD UNICO
        // ================================
        private void StartPollingThread()
        {
            if (_pollingActive) return; // Già attivo

            _pollingActive = true;
            _pollingThread = new Thread(ContinuousPollingThread)
            {
                Name = "InputPollingThread",
                IsBackground = true,
                Priority = ThreadPriority.Highest // Alta priorità per non perdere input
            };
            _pollingThread.Start();

            lblPollingStatus.Text = "Polling: THREAD UNICO ATTIVO";
            lblPollingStatus.ForeColor = Color.Green;

            Console.WriteLine("✓ Thread di polling unico avviato!");
        }

        private void StopPollingThread()
        {
            _pollingActive = false;
            if (_pollingThread != null && _pollingThread.IsAlive)
            {
                _pollingThread.Join(500); // Aspetta max 0.5 secondi
            }
            _pollingThread = null;

            lblPollingStatus.Text = "Polling: FERMO";
            lblPollingStatus.ForeColor = Color.Red;
            Console.WriteLine("✓ Thread di polling fermato.");
        }

        // ===============================================
        // === THREAD DI POLLING - FINALE E CORRETTO ===
        // ===============================================
        private void ContinuousPollingThread()
        {
            Console.WriteLine($"[THREAD] '{Thread.CurrentThread.Name}' AVVIATO (Ottimizzato 1000Hz)");

            var inputsToShow = new System.Collections.Generic.List<string>();
            int loopCount = 0;
            int consecutiveEmptyReads = 0;
            int totalEventsThisSecond = 0;
            DateTime lastStatsLog = DateTime.Now;

            while (_pollingActive)
            {
                try
                {
                    loopCount++;

                    // *** POLLING AGGRESSIVO - Leggi SEMPRE, non aspettare stats ***
                    // Questo è il cambio critico: non controlliamo più TotalInputs

                    // *** LETTURA MASSIVA: 1000 eventi per volta ***
                    var inputs = _driver.GetInputData(1000); // ← ERA 250, ORA 1000!

                    if (inputs.Length > 0)
                    {
                        // Reset counter empty reads
                        consecutiveEmptyReads = 0;

                        // Aggiorna contatori
                        long newTotal = Interlocked.Add(ref _totalEventsReadByThread, inputs.Length);
                        totalEventsThisSecond += inputs.Length;

                        // Log ogni 100 eventi
                        if (newTotal % 100 == 0)
                        {
                            Console.WriteLine($"[THREAD] ✓ Letti {inputs.Length} eventi (Totale: {newTotal})");
                        }

                        // Formatta SOLO gli ultimi 50 per non sovraccaricare l'UI
                        int eventsToShow = Math.Min(inputs.Length, 50);

                        for (int i = 0; i < eventsToShow; i++)
                        {
                            inputsToShow.Add(FormatInputData(inputs[i]));
                        }

                        // Se ci sono più di 50, mostra un riepilogo
                        if (inputs.Length > 50)
                        {
                            inputsToShow.Add($"... +{inputs.Length - 50} altri eventi (nascosti per performance)");
                        }
                    }
                    else
                    {
                        consecutiveEmptyReads++;

                        // Se abbiamo molte letture vuote consecutive, rallenta leggermente
                        if (consecutiveEmptyReads > 100)
                        {
                            Thread.Sleep(1); // Sleep 1ms solo se davvero non ci sono dati
                            consecutiveEmptyReads = 0; // Reset
                        }
                    }

                    // *** AGGIORNA UI SE HAI DATI ***
                    if (inputsToShow.Count > 0)
                    {
                        try
                        {
                            this.Invoke((Action)delegate
                            {
                                // Inserisci in testa (ultimi eventi in alto)
                                for (int i = inputsToShow.Count - 1; i >= 0; i--)
                                {
                                    lstInputs.Items.Insert(0, inputsToShow[i]);
                                }
                                inputsToShow.Clear();

                                // Mantieni solo 200 items per performance
                                while (lstInputs.Items.Count > 200)
                                {
                                    lstInputs.Items.RemoveAt(lstInputs.Items.Count - 1);
                                }

                                lblPollingStatus.Text = "Polling: ATTIVO 1000Hz";
                                lblPollingStatus.ForeColor = Color.Green;
                            });
                        }
                        catch (Exception invEx)
                        {
                            Console.WriteLine($"[THREAD] Errore Invoke: {invEx.Message}");
                        }
                    }

                    // *** STATISTICHE OGNI SECONDO ***
                    if ((DateTime.Now - lastStatsLog).TotalSeconds >= 1.0)
                    {
                        Console.WriteLine($"[STATS] Eventi/sec letti: {totalEventsThisSecond}, Loop/sec: {loopCount}");
                        totalEventsThisSecond = 0;
                        loopCount = 0;
                        lastStatsLog = DateTime.Now;
                    }

                    // *** NO SLEEP - Polling continuo massima velocità ***
                    // Thread.Sleep(0) fa yield, ma rallenta. Evitiamolo.
                    // Se CPU usage è troppo alto, usa: Thread.SpinWait(10);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[THREAD] ERRORE CRITICO: {ex.Message}");
                    _pollingActive = false;

                    this.Invoke((Action)delegate
                    {
                        lblPollingStatus.Text = "Polling: ERRORE CRITICO";
                        lblPollingStatus.ForeColor = Color.Red;
                    });
                }
            }

            Console.WriteLine($"[THREAD] '{Thread.CurrentThread.Name}' TERMINATO");
            Console.WriteLine($"[THREAD] Totale eventi letti: {_totalEventsReadByThread}");
        }


        // ================================
        // ALTRE FUNZIONI
        // ================================
        private void BtnApplyPolicy_Click(object sender, EventArgs e)
        {
            var policy = new BlockPolicy(true);

            policy.BlockKeyboard = (byte)(chkBlockKeyboard.Checked ? 1 : 0);
            policy.BlockMouse = (byte)(chkBlockMouse.Checked ? 1 : 0);
            policy.BlockMouseButtons = (byte)(chkBlockMouseButtons.Checked ? 1 : 0);
            policy.BlockMouseMovement = (byte)(chkBlockMouseMovement.Checked ? 1 : 0);

            if (!string.IsNullOrWhiteSpace(txtSpecificKeys.Text))
            {
                try
                {
                    var scanCodes = txtSpecificKeys.Text.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(int.Parse);

                    foreach (var scanCode in scanCodes)
                    {
                        if (scanCode >= 0 && scanCode < 256)
                        {
                            policy.BlockSpecificKeys[scanCode] = 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore nel parsing dei scan codes: {ex.Message}",
                                  "Errore", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            ApplyGlobalPolicy(policy);
        }

        private void BtnClearInputs_Click(object sender, EventArgs e)
        {
            lstInputs.Items.Clear();
            Console.WriteLine("Lista input pulita.");
        }

        // =======================================================
        // === TIMER STATISTICHE - SOLO VISUALIZZAZIONE ===
        // =======================================================
        private void StatsTimer_Tick(object sender, EventArgs e)
        {
            var stats = _driver.GetStatistics();
            if (stats.HasValue)
            {
                var s = stats.Value;
                lblTotalInputs.Text = $"Input Totali: {s.TotalInputs}";
                lblBlockedInputs.Text = $"Input Bloccati: {s.BlockedInputs}";
                lblKeyboardInputs.Text = $"Input Tastiera: {s.KeyboardInputs}";
                lblMouseInputs.Text = $"Input Mouse: {s.MouseInputs}";

                // *** CALCOLO CAPTURE RATE ***
                long eventsRead = _totalEventsReadByThread;
                lblEventsRead.Text = $"Eventi Letti C#: {eventsRead}";

                if (s.TotalInputs > 0 && eventsRead > 0)
                {
                    double captureRate = (eventsRead / (double)s.TotalInputs) * 100.0;
                    lblCaptureRate.Text = $"Capture Rate: {captureRate:F2}%";

                    if (captureRate >= 95.0)
                    {
                        lblCaptureRate.ForeColor = Color.Green;
                        lblCaptureRate.Text += " ✓ OTTIMO";
                    }
                    else if (captureRate >= 85.0)
                    {
                        lblCaptureRate.ForeColor = Color.Orange;
                        lblCaptureRate.Text += " ⚠ ACCETTABILE";
                    }
                    else if (captureRate >= 50.0)
                    {
                        lblCaptureRate.ForeColor = Color.OrangeRed;
                        lblCaptureRate.Text += " ⚠ PERDITA MEDIA";
                    }
                    else
                    {
                        lblCaptureRate.ForeColor = Color.Red;
                        lblCaptureRate.Text += " ✗ PERDITA CRITICA!";

                        // *** SUGGERIMENTI AUTOMATICI ***
                        if (s.TotalInputs - eventsRead > 5000)
                        {
                            lblCaptureRate.Text += " - AUMENTA BUFFER DRIVER!";
                        }
                    }
                }
                else
                {
                    lblCaptureRate.Text = "Capture Rate: N/A";
                    lblCaptureRate.ForeColor = Color.Gray;
                }

                // Sincronizza stato
                if (s.InterceptEnabled != 0 && !_pollingActive)
                {
                    StartPollingThread();
                }
                else if (s.InterceptEnabled == 0 && _pollingActive)
                {
                    StopPollingThread();
                }

                lblInterceptStatus.Text = s.InterceptEnabled != 0 ? "Intercettazione: Abilitata" : "Intercettazione: Disabilitata";
                lblInterceptStatus.ForeColor = s.InterceptEnabled != 0 ? Color.Green : Color.Red;
            }

            CheckPollingHealth();
        }

        // =================================================
        // === CONTROLLO SALUTE - SEMPLIFICATO ===
        // =================================================
        private void CheckPollingHealth()
        {
            if (_pollingActive)
            {
                double secondsElapsed = (DateTime.Now - _lastStatsTime).TotalSeconds;
                if (secondsElapsed < 1) return; // Non aggiornare troppo spesso

                long currentEventsRead = _totalEventsReadByThread;
                long eventsInLastSecond = currentEventsRead - _lastPollingCountForStats;
                double frequency = eventsInLastSecond / secondsElapsed;

                lblPollingCalls.Text = $"Eventi letti: {currentEventsRead} (Freq: {frequency:F0} Hz)";

                // Aggiorna i contatori per il prossimo tick
                _lastPollingCountForStats = currentEventsRead;
                _lastStatsTime = DateTime.Now;
            }
        }

        private string FormatInputData(InterceptedInput input)
        {
            string timeString;
            try
            {
                timeString = DateTime.FromFileTimeUtc(input.Timestamp).ToLocalTime().ToString("HH:mm:ss.fff");
            }
            catch (ArgumentOutOfRangeException)
            {
                timeString = "INVALID_TIME";
            }

            var blocked = input.IsBlocked ? "BLOCCATO" : "PASSATO";
            string formattedString = $"{timeString} ";

            switch (input.InputType)
            {
                case InputType.InputKeyboard:
                    var keyDown = input.KeyboardKeyDown ? "DOWN" : "UP  ";
                    formattedString += $"[Tastiera] Scan: 0x{input.KeyboardScanCode:X2} ({input.KeyboardScanCode,3}) | {keyDown} | {blocked}";
                    break;

                case InputType.InputMouseButton:
                    formattedString += $"[Mouse Btn] X:{input.MouseX} Y:{input.MouseY} Flags:0x{input.MouseButtonFlags:X4} | {blocked}";
                    break;

                case InputType.InputMouseMove:
                    formattedString += $"[Mouse Move] X:{input.MouseX} Y:{input.MouseY} | {blocked}";
                    break;

                case InputType.InputMouseWheel:
                    formattedString += $"[Mouse Wheel] Data:{input.MouseButtonData} | {blocked}";
                    break;

                default:
                    formattedString += $"[Sconosciuto] Tipo:{input.Type} | {blocked}";
                    break;
            }

            return formattedString;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Console.WriteLine("Chiusura applicazione...");
            StopPollingThread();
            _statsTimer?.Stop();
            _driver?.Dispose();
            base.OnFormClosing(e);
        }
    }
}