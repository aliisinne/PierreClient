using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using PierreLauncher.Models;
using PierreLauncher.Services;

namespace PierreLauncher
{
    public partial class MainWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly MinecraftLaunchService _launchService;
        private DiscordRPC.DiscordRpcClient _rpcClient;
        private bool _isLaunching = false;

        public MainWindow()
        {
            InitializeComponent();

            _configService = new ConfigService();
            _launchService = new MinecraftLaunchService(_configService);

            _launchService.StatusChanged += OnStatusChanged;
            _launchService.ProgressChanged += OnProgressChanged;

            InitializeDiscordRpc();

            LoadLogos();
            LoadData();

            // First run check: If no account exists, prompt account view or account creation immediately
            if (_configService.Config.Accounts.Count == 0)
            {
                ShowAccountSelectionView();
            }
        }

        private void InitializeDiscordRpc()
        {
            _rpcClient = new DiscordRPC.DiscordRpcClient("1269986326162280450");
            _rpcClient.Initialize();
            
            _rpcClient.SetPresence(new DiscordRPC.RichPresence()
            {
                Details = "Launcher'da",
                State = "Oyuna Hazırlanıyor..."
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_rpcClient != null)
            {
                _rpcClient.Dispose();
            }
            base.OnClosed(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
            this.BeginAnimation(Window.OpacityProperty, anim);
        }

        private void LoadLogos()
        {
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo_p.png");
                if (File.Exists(logoPath))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(logoPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();

                    ImgHeaderLogo.Source = bmp;
                    ImgCenterLogo.Source = bmp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logo load error: {ex.Message}");
            }
        }

        private void LoadData()
        {
            var activeAcc = _configService.GetActiveAccount();
            if (activeAcc != null && !string.IsNullOrWhiteSpace(activeAcc.Username))
            {
                TxtActiveUser.Text = activeAcc.Username;
            }
            else
            {
                TxtActiveUser.Text = "Hesap Seçilmedi";
            }

            TxtGameDirectory.Text = _configService.Config.GameDirectory;
            TxtJvmArgs.Text = _configService.Config.JvmArguments;
            SliderMemoryGb.Value = _configService.Config.RamAllocationGb;
            TxtRamDisplay.Text = $"{_configService.Config.RamAllocationGb:0.0} GB";

            ChkMinimizeOnLaunch.IsChecked = _configService.Config.MinimizeOnLaunch;
            ChkCloseOnLaunch.IsChecked = _configService.Config.CloseOnLaunch;

            // Load Language
            if (_configService.Config.SelectedLanguage == "English")
                CmbLanguage.SelectedIndex = 1;
            else
                CmbLanguage.SelectedIndex = 0;

            // Load Branch
            if (_configService.Config.SelectedBranch == "release")
                CmbBranch.SelectedIndex = 1;
            else
                CmbBranch.SelectedIndex = 0;

            ApplyLanguage();
            RenderAccountsList();
        }

        private void ApplyLanguage()
        {
            bool isEnglish = _configService.Config.SelectedLanguage == "English";
            
            // Translations
            TxtStatus.Text = isEnglish ? "Ready to start the game." : "Oyuna başlanmaya hazır.";
            TxtActiveUser.Text = string.IsNullOrWhiteSpace(_configService.GetActiveAccount()?.Username) 
                ? (isEnglish ? "No Account Selected" : "Kullanıcı Seçilmedi") 
                : _configService.GetActiveAccount()?.Username;

            BtnPlay.Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Children = 
                {
                    new TextBlock { Text = "▶", Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0x14, 0x1D)), FontSize = 20, Margin = new Thickness(0,0,10,0), VerticalAlignment = VerticalAlignment.Center },
                    new StackPanel 
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Children = 
                        {
                            new TextBlock { Text = isEnglish ? "LAUNCH 1.21.11" : "BAŞLAT 1.21.11", Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0x14, 0x1D)), FontWeight = FontWeights.Black, FontSize = 17 },
                            new TextBlock { Text = isEnglish ? "PIERRE CLIENT FABRIC MODDED" : "PIERRE CLIENT FABRIC MODLU", Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0x14, 0x1D)), FontSize = 9, FontWeight = FontWeights.Bold }
                        }
                    }
                }
            };

            ChkMinimizeOnLaunch.Content = isEnglish ? "Minimize on game launch" : "Oyun açılınca küçült";
            ChkCloseOnLaunch.Content = isEnglish ? "Close on game launch" : "Oyun açılınca kapat";

            if (TxtQuickPlayLabel != null) TxtQuickPlayLabel.Text = isEnglish ? "Quick Play:" : "Hızlı Oyun:";
            if (TxtQuickPlayStatus != null) TxtQuickPlayStatus.Text = isEnglish ? "(No Server Added)" : "(Sunucu Eklenmedi)";
            if (TxtNewsTitle != null) TxtNewsTitle.Text = isEnglish ? "Latest News" : "Son Haberler";
            if (TxtSettingsHeader != null) TxtSettingsHeader.Text = isEnglish ? "Launcher settings" : "Başlatıcı Ayarları";
            if (TxtLanguageLabel != null) TxtLanguageLabel.Text = isEnglish ? "Language:" : "Dil:";
            if (TxtBranchLabel != null) TxtBranchLabel.Text = isEnglish ? "Branch:" : "Sürüm Dalı:";
            if (TxtMemoryLabel != null) TxtMemoryLabel.Text = isEnglish ? "Memory:" : "RAM Bellek:";
            if (TxtGameDirLabel != null) TxtGameDirLabel.Text = isEnglish ? "Game directory:" : "Oyun Dizini:";
            if (TxtJvmArgsLabel != null) TxtJvmArgsLabel.Text = isEnglish ? "JVM arguments:" : "JVM Argümanları:";
            if (BtnSaveText != null) BtnSaveText.Text = isEnglish ? "SAVE" : "KAYDET";
            if (BtnGoBack != null) BtnGoBack.Content = isEnglish ? "Go Back" : "Geri Dön";
            if (TxtAddAccountTitle != null) TxtAddAccountTitle.Text = isEnglish ? "Add Account" : "Hesap Ekle";
            if (TxtAddAccountDesc != null) TxtAddAccountDesc.Text = isEnglish ? "👤 Username (Nickname):" : "👤 Kullanıcı Adı (Nickname):";
            if (TxtBtnAddAccount != null) TxtBtnAddAccount.Text = isEnglish ? "ADD OFFLINE ACCOUNT" : "ÇEVRİMDIŞI HESAP EKLE";

            if (TxtWelcomeSubtitle != null) TxtWelcomeSubtitle.Text = isEnglish ? "Play and enjoy a better experience!" : "Oynayın ve daha iyi bir deneyimin tadını çıkarın!";
            if (TxtAccountSelectHeader != null) TxtAccountSelectHeader.Text = isEnglish ? "Choose an account" : "Bir hesap seçin";
            if (TxtAddAccountBtn != null) TxtAddAccountBtn.Text = isEnglish ? "Add Account" : "Hesap Ekle";
            if (TxtJavaBadge != null) TxtJavaBadge.Text = isEnglish ? "Java 21 (Default - Fixed)" : "Java 21 (Varsayılan - Sabit)";

            // News Translations
            if (TxtNews1Title != null) TxtNews1Title.Text = isEnglish ? "Pierre Client is Live!" : "Pierre Client Yayında!";
            if (TxtNews1Desc != null) TxtNews1Desc.Text = isEnglish ? "The brand new Pierre Client with 1.21.11 Fabric infrastructure is now available. Mods are integrated, stability is improved." : "1.21.11 Fabric altyapısıyla yep yeni Pierre Client sürümü artık erişilebilir. Modlar entegre edildi, stabilite artırıldı.";
            if (TxtNews1Date != null) TxtNews1Date.Text = isEnglish ? "Today, 12:00" : "Bugün, 12:00";
            
            if (TxtNews2Title != null) TxtNews2Title.Text = isEnglish ? "Anti-Tamper Active" : "Anti-Tamper Aktif";
            if (TxtNews2Desc != null) TxtNews2Desc.Text = isEnglish ? "Security system preventing unauthorized mod additions is activated. The client is now much more secure." : "İzinsiz mod eklemeyi önleyen güvenlik sistemi aktifleştirildi. Client artık çok daha güvenli.";
            if (TxtNews2Date != null) TxtNews2Date.Text = isEnglish ? "Yesterday, 18:30" : "Dün, 18:30";
        }

        private void RenderAccountsList()
        {
            AccountsContainer.Children.Clear();

            if (_configService.Config.Accounts.Count == 0)
            {
                CardNoAccounts.Visibility = Visibility.Visible;
                ScrollAccountsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                CardNoAccounts.Visibility = Visibility.Collapsed;
                ScrollAccountsList.Visibility = Visibility.Visible;

                foreach (var acc in _configService.Config.Accounts)
                {
                    var card = CreateAccountCard(acc);
                    AccountsContainer.Children.Add(card);
                }
            }
        }

        private UIElement CreateAccountCard(Account acc)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(acc.IsActive ? Color.FromArgb(0xC0, 0x15, 0x22, 0x1C) : Color.FromArgb(0x60, 0x0D, 0x11, 0x1A)),
                BorderBrush = new SolidColorBrush(acc.IsActive ? Color.FromRgb(0x00, 0xE6, 0x76) : Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(acc.IsActive ? 1.5 : 1),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Avatar Icon Box
            var avatarBorder = new Border
            {
                Width = 38,
                Height = 38,
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(0x00, 0xE6, 0x76)),
                Margin = new Thickness(0, 0, 12, 0)
            };
            var avatarIcon = new TextBlock
            {
                Text = "🐸",
                FontSize = 22,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarBorder.Child = avatarIcon;
            Grid.SetColumn(avatarBorder, 0);
            grid.Children.Add(avatarBorder);

            // User Info Text
            var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var usernameText = new TextBlock
            {
                Text = acc.Username,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };
            bool isEnglish = _configService.Config.SelectedLanguage == "English";
            var timeSince = DateTime.Now - acc.LastLoginTime;
            string displayLastLogin = "";
            if (timeSince.TotalMinutes < 1)
                displayLastLogin = isEnglish ? "Last login: a few seconds ago" : "Son giriş: birkaç saniye önce";
            else
                displayLastLogin = isEnglish ? $"Last login: {(int)timeSince.TotalMinutes} minutes ago" : $"Son giriş: {(int)timeSince.TotalMinutes} dakika önce";

            var subText = new TextBlock
            {
                Text = displayLastLogin,
                Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0xAA, 0xB0)),
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 0)
            };
            infoStack.Children.Add(usernameText);
            infoStack.Children.Add(subText);
            Grid.SetColumn(infoStack, 1);
            grid.Children.Add(infoStack);

            // Delete Trash Button 🗑
            var deleteBtn = new Button
            {
                Content = "🗑",
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x52, 0x52)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(6)
            };
            deleteBtn.Click += (s, e) =>
            {
                e.Handled = true;
                _configService.RemoveAccount(acc.Username);
                LoadData();
            };
            Grid.SetColumn(deleteBtn, 2);
            grid.Children.Add(deleteBtn);

            border.Child = grid;

            // Click card to select account
            border.MouseLeftButtonDown += (s, e) =>
            {
                _configService.SetActiveAccount(acc.Username);
                LoadData();
                ShowMainLaunchView();
            };

            return border;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowMainLaunchView()
        {
            AccountSelectionOverlay.Visibility = Visibility.Collapsed;
            MainLaunchView.Visibility = Visibility.Visible;
            AnimateFadeIn(MainLaunchView);
        }

        private void ShowAccountSelectionView()
        {
            RenderAccountsList();
            MainLaunchView.Visibility = Visibility.Collapsed;
            AccountSelectionOverlay.Visibility = Visibility.Visible;
            AnimateFadeIn(AccountSelectionOverlay);
        }

        private void ShowAccountSelection_Click(object? sender, RoutedEventArgs? e)
        {
            ShowAccountSelectionView();
        }

        private void OpenAddAccountModal_Click(object sender, RoutedEventArgs e)
        {
            TxtInputUsername.Clear();
            AddAccountModalOverlay.Visibility = Visibility.Visible;
            AnimateFadeIn(AddAccountModalOverlay);
        }

        private void CloseAddAccountModal_Click(object sender, RoutedEventArgs e)
        {
            AddAccountModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void AddOfflineAccount_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtInputUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Lütfen oyuna girmek için bir kullanıcı adı (nickname) yazın.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _configService.AddAccount(username, "Çevrimdışı");
            AddAccountModalOverlay.Visibility = Visibility.Collapsed;
            ShowMainLaunchView();
            LoadData();
        }

        private void ToggleSettingsDrawer_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsDrawer.Visibility == Visibility.Visible)
            {
                CloseSettingsDrawer_Click(sender, e);
            }
            else
            {
                AnimatePanelTransition(NewsPanel, SettingsDrawer, NewsPanelTransform, SettingsDrawerTransform, true);
            }
        }

        private void CloseSettingsDrawer_Click(object sender, RoutedEventArgs e)
        {
            AnimatePanelTransition(SettingsDrawer, NewsPanel, SettingsDrawerTransform, NewsPanelTransform, false);
        }

        private void AnimatePanelTransition(UIElement hidePanel, UIElement showPanel, TranslateTransform hideTransform, TranslateTransform showTransform, bool slideLeft)
        {
            // Fade out and slide out old panel
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, ev) => hidePanel.Visibility = Visibility.Collapsed;
            hidePanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            var slideOut = new DoubleAnimation(slideLeft ? -50 : 50, TimeSpan.FromMilliseconds(200));
            hideTransform.BeginAnimation(TranslateTransform.XProperty, slideOut);

            // Fade in and slide in new panel
            showPanel.Visibility = Visibility.Visible;
            showPanel.Opacity = 0;
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300)) { BeginTime = TimeSpan.FromMilliseconds(100) };
            showPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            showTransform.X = slideLeft ? 50 : -50;
            var slideIn = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)) { BeginTime = TimeSpan.FromMilliseconds(100), EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
            showTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);
        }

        private void SliderMemoryGb_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtRamDisplay != null)
            {
                double gb = Math.Round(e.NewValue * 2) / 2.0;
                TxtRamDisplay.Text = $"{gb:0.0} GB";
            }
        }

        private void BrowseGameDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Minecraft Klasörünü Seçin"
            };
            if (dialog.ShowDialog() == true)
            {
                TxtGameDirectory.Text = dialog.FolderName;
            }
        }

        private void ResetGameDir_Click(object sender, RoutedEventArgs e)
        {
            TxtGameDirectory.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ".minecraft"
            );
        }

        private void ResetJvmArgs_Click(object sender, RoutedEventArgs e)
        {
            TxtJvmArgs.Text = "-XX:+UnlockExperimentalVMOptions -XX:+UseG1GC";
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _configService.Config.RamAllocationGb = Math.Round(SliderMemoryGb.Value * 2) / 2.0;
            _configService.Config.GameDirectory = TxtGameDirectory.Text.Trim();
            _configService.Config.JvmArguments = TxtJvmArgs.Text.Trim();
            _configService.Config.MinimizeOnLaunch = ChkMinimizeOnLaunch.IsChecked ?? true;
            _configService.Config.CloseOnLaunch = ChkCloseOnLaunch.IsChecked ?? false;
            
            _configService.Config.SelectedJava = "Java 21";

            if (CmbLanguage.SelectedItem is ComboBoxItem itemLang)
                _configService.Config.SelectedLanguage = itemLang.Content.ToString() ?? "Türkçe";

            if (CmbBranch.SelectedItem is ComboBoxItem itemBranch)
                _configService.Config.SelectedBranch = itemBranch.Content.ToString() ?? "master";

            _configService.Save();
            ApplyLanguage();
            CloseSettingsDrawer_Click(null, null);
            ShowSnackbar(CmbLanguage.SelectedIndex == 1 ? "Settings saved successfully!" : "Ayarlar başarıyla kaydedildi!");
        }

        private async void ShowSnackbar(string message)
        {
            if (TxtSnackbarMessage != null) TxtSnackbarMessage.Text = message;
            if (SnackbarPanel != null && SnackbarTransform != null)
            {
                SnackbarPanel.Visibility = Visibility.Visible;
                
                // Fade In & Slide Up
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
                var slideUp = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut } };
                
                SnackbarPanel.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                SnackbarTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);

                // Wait 2.5 seconds
                await Task.Delay(2500);

                // Fade Out & Slide Down
                var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn } };
                var slideDown = new DoubleAnimation(20, TimeSpan.FromMilliseconds(300)) { EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseIn } };
                
                fadeOut.Completed += (s, ev) => SnackbarPanel.Visibility = Visibility.Collapsed;
                
                SnackbarPanel.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                SnackbarTransform.BeginAnimation(TranslateTransform.YProperty, slideDown);
            }
        }

        private async void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_isLaunching) return;

            var activeAccount = _configService.GetActiveAccount();
            if (activeAccount == null || string.IsNullOrWhiteSpace(activeAccount.Username))
            {
                ShowAccountSelectionView();
                return;
            }

            _isLaunching = true;
            BtnPlay.IsEnabled = false;
            LaunchProgressBar.Visibility = Visibility.Visible;
            LaunchProgressBar.Value = 0;

            if (_rpcClient != null)
            {
                _rpcClient.SetPresence(new DiscordRPC.RichPresence()
                {
                    Details = "Oyun Açık",
                    State = $"Hesap: {activeAccount.Username}",
                    Timestamps = DiscordRPC.Timestamps.Now
                });
            }

            try
            {
                var process = await _launchService.LaunchAsync();

                // Show the Game Loading Overlay while waiting for Minecraft to initialize
                GameLoadingOverlay.Visibility = Visibility.Visible;
                AnimateFadeIn(GameLoadingOverlay);

                // Wait for the game process to actually display its window
                await Task.Run(async () =>
                {
                    while (!process.HasExited)
                    {
                        process.Refresh();
                        if (process.MainWindowHandle != IntPtr.Zero)
                        {
                            break;
                        }
                        await Task.Delay(500);
                    }
                });

                // Wait an additional 2 seconds after the window is found just to make the transition smooth
                await Task.Delay(2000);

                if (_configService.Config.CloseOnLaunch)
                {
                    this.Close();
                }
                else if (_configService.Config.MinimizeOnLaunch)
                {
                    GameLoadingOverlay.Visibility = Visibility.Collapsed;
                    this.WindowState = WindowState.Minimized;
                }
                else
                {
                    GameLoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Başlatma Hatası:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Başlatma Hatası. Ayarları kontrol edin.";
            }
            finally
            {
                _isLaunching = false;
                BtnPlay.IsEnabled = true;
                LaunchProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void OnStatusChanged(string status)
        {
            Dispatcher.Invoke(() =>
            {
                TxtStatus.Text = status;
            });
        }

        private void OnProgressChanged(int progress)
        {
            Dispatcher.Invoke(() =>
            {
                LaunchProgressBar.Value = progress;
            });
        }

        // --- STORYBOARD ANIMATIONS ---
        private void AnimateFadeIn(UIElement element)
        {
            element.Opacity = 0;
            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            element.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        private void AnimateSlideFromRight(TranslateTransform transform)
        {
            transform.X = 330;
            DoubleAnimation slideIn = new DoubleAnimation
            {
                From = 330,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 }
            };
            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
        }
    }
}