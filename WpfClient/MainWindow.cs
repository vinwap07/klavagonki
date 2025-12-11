using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System;

namespace WpfClient
{
    public partial class MainWindow : Window
    {
        private readonly GameClientFacade _gameClient;
        private readonly ObservableCollection<RoomInfo> _rooms = new();
        private readonly ObservableCollection<PlayerProgress> _playersProgress = new();
        private int _currentPosition = 0;
        private DateTime _gameStartTime;
        private int _errorsCount = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            
            _gameClient = new GameClientFacade();
            listRooms.ItemsSource = _rooms;
            playersProgress.ItemsSource = _playersProgress;
            
            SetupEventHandlers();
        }
        
        private void SetupEventHandlers()
        {
            // Подписываемся на события клиента
            _gameClient.OnConnectedToServer += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtConnectionStatus.Text = "Подключено";
                    btnConnect.IsEnabled = false;
                    btnDisconnect.IsEnabled = true;
                    btnCreateRoom.IsEnabled = true;
                    btnRefreshRooms.IsEnabled = true;
                    txtPlayerName.IsEnabled = false;
                    
                    // Запрашиваем список комнат
                    _gameClient.GetRoomsAsync();
                });
            };
            
            _gameClient.OnDisconnected += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtConnectionStatus.Text = "Не подключено";
                    btnConnect.IsEnabled = true;
                    btnDisconnect.IsEnabled = false;
                    btnCreateRoom.IsEnabled = false;
                    btnRefreshRooms.IsEnabled = false;
                    txtPlayerName.IsEnabled = true;
                    _rooms.Clear();
                });
            };
            
            _gameClient.OnRoomsListReceived += (rooms) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _rooms.Clear();
                    foreach (var room in rooms)
                    {
                        _rooms.Add(room);
                    }
                });
            };
            
            _gameClient.OnRoomJoined += (roomId) =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtCurrentRoom.Text = roomId;
                    txtSelectedRoom.Text = roomId;
                    btnReady.Visibility = Visibility.Visible;
                    btnReady.IsEnabled = true;
                    btnJoinRoom.IsEnabled = false;
                    tabGame.IsEnabled = true;
                    tabControl.SelectedItem = tabGame;
                });
            };
            
            _gameClient.OnGameTextReceived += (text) =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtRaceText.Text = text;
                    txtInput.IsEnabled = true;
                    txtInput.Focus();
                    _currentPosition = 0;
                    _errorsCount = 0;
                    _gameStartTime = DateTime.Now;
                    UpdateTextColors();
                });
            };
            
            _gameClient.OnGameStarted += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtGameStatus.Text = "Игра началась! Печатайте текст выше.";
                    btnReady.Visibility = Visibility.Collapsed;
                });
            };
            
            _gameClient.OnProgressUpdated += (progresses) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _playersProgress.Clear();
                    foreach (var progress in progresses)
                    {
                        _playersProgress.Add(new PlayerProgress
                        {
                            PlayerName = progress.Key,
                            Progress = progress.Value
                        });
                    }
                    
                    // Обновляем счетчик игроков
                    txtOnlineCount.Text = progresses.Count.ToString();
                });
            };
            
            _gameClient.OnGameFinished += (result) =>
            {
                Dispatcher.Invoke(() =>
                {
                    txtInput.IsEnabled = false;
                    txtGameStatus.Text = $"Игра окончена! Победитель: {result.Winner} " +
                                       $"Время: {result.Time:F2}с Точность: {result.Accuracy:F1}%";
                    
                    MessageBox.Show($"Победитель: {result.Winner}\n" +
                                  $"Ваше время: {result.Time:F2} секунд\n" +
                                  $"Точность: {result.Accuracy:F1}%",
                                  "Результаты игры");
                });
            };
            
            _gameClient.OnError += (error) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            };
        }
        
        // ===== ОБРАБОТЧИКИ СОБЫТИЙ UI =====
        
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _gameClient.PlayerName = txtPlayerName.Text;
                await _gameClient.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _gameClient.Disconnect();
        }
        
        private async void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _gameClient.CreateRoomAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания комнаты: {ex.Message}", "Ошибка");
            }
        }
        
        private async void btnRefreshRooms_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _gameClient.GetRoomsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления списка: {ex.Message}", "Ошибка");
            }
        }
        
        private void listRooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listRooms.SelectedItem is RoomInfo selectedRoom)
            {
                txtSelectedRoom.Text = selectedRoom.Id;
                btnJoinRoom.IsEnabled = true;
            }
            else
            {
                btnJoinRoom.IsEnabled = false;
            }
        }
        
        private async void btnJoinRoom_Click(object sender, RoutedEventArgs e)
        {
            if (listRooms.SelectedItem is RoomInfo selectedRoom)
            {
                try
                {
                    await _gameClient.JoinRoomAsync(selectedRoom.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка присоединения: {ex.Message}", "Ошибка");
                }
            }
        }
        
        private async void btnReady_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _gameClient.SendReadyAsync();
                btnReady.IsEnabled = false;
                txtGameStatus.Text = "Ожидаем других игроков...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        
        private async void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter || 
                (e.Key >= Key.A && e.Key <= Key.Z) ||
                (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                char typedChar = GetCharFromKey(e.Key, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
                
                try
                {
                    await _gameClient.SendCharAsync(typedChar, _currentPosition);
                    _currentPosition++;
                    
                    // Обновляем цвета текста
                    UpdateTextColors();
                    
                    // Обновляем статистику
                    UpdateStatistics();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка отправки символа: {ex.Message}", "Ошибка");
                }
            }
        }
        
        private void UpdateTextColors()
        {
            // Здесь нужно реализовать подсветку текста
            // Правильные символы - зеленые, ошибки - красные
            // Это требует работы с RichTextBox вместо TextBlock
        }
        
        private void UpdateStatistics()
        {
            var elapsed = DateTime.Now - _gameStartTime;
            if (elapsed.TotalSeconds > 0)
            {
                double speed = (_currentPosition / elapsed.TotalSeconds) * 60;
                txtTypingSpeed.Text = $"Скорость: {speed:F0} зн/мин";
            }
            txtErrors.Text = $"Ошибок: {_errorsCount}";
        }
        
        private char GetCharFromKey(Key key, bool isShiftPressed)
        {
            // Простая конвертация клавиш в символы
            if (key >= Key.A && key <= Key.Z)
            {
                char c = (char)('a' + (key - Key.A));
                return isShiftPressed ? char.ToUpper(c) : c;
            }
            else if (key == Key.Space)
            {
                return ' ';
            }
            else if (key == Key.Enter)
            {
                return '\n';
            }
            // Добавь обработку других символов по необходимости
            
            return ' ';
        }
        
        private async void btnLeaveGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _gameClient.LeaveRoomAsync();
                tabGame.IsEnabled = false;
                tabControl.SelectedItem = tabControl.Items[0]; // Возвращаемся в лобби
                btnReady.Visibility = Visibility.Collapsed;
                btnJoinRoom.IsEnabled = false;
                txtSelectedRoom.Text = "нет";
                _playersProgress.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _gameClient.Dispose();
        }
    }
    
    // Вспомогательные классы для отображения
    public class PlayerProgress
    {
        public string PlayerName { get; set; } = string.Empty;
        public double Progress { get; set; }
    }
    
    // Конвертер для статуса комнаты
    public class StatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isPlaying)
            {
                return isPlaying ? "В игре" : "Ожидание";
            }
            return "Неизвестно";
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}