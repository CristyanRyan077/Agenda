   M S B A M L   `   `   `  ����E  AAgendaNovo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:AgendaNovo
AgendaNovo  5          AgendaNovo.Login   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      *localclr-namespace:AgendaNovo     5      S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5      _ [MaterialDesignThemes.Wpf, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5      
������   � AllowsTransparency$
  True��5       � Title$ Login��5      $	��900��6   $
��1440��6     � WindowStyle$
 None=�5       � WindowState$ 	Maximized=�5       � 
ResizeMode$ NoResize=�5       � WindowStartupLocation$ CenterScreen=�5      .��5      ��  �� CornerRadius$ 10��6   ��5   
   �� 5       �� ImageSource$ /Imagens/background2.jpg��6   ._�5   
   �� $ 10��6   $��1q�6#   ._�5      � .+�5         HandyControl.Controls.Card  -   5        $��	cardLogin��5      $	��600��6+   $
��auto��67   $��20q�6E   $��Center=�6R   
������ 
������ .��5      � ���	#88000000 � RowDefinitions 5      �� 5      $	��252 �6,   �� 5      $	��302 �6,   �� 5      $
��Auto �6,   �� 5       $
��Auto �6,   �� 5!      $
��Auto �6,   �� 5"      $
��Auto �6,   .+�5%      �� -   5        $��	imgstudio��5%       $��/Imagens/SÓ LOGO.png��61   $
��Left=�6O   $��0��5&       $	��270��6-   $	��250��69   $��	136,1,0,1q�6F   �� 5(      -   5        	 ��Name$	 lblNome��5(       $��Usuario��61   $��	16,23,0,0q�5)       
������ 
�������$��16(�5+       $��59��6.   $��Bahnschrift)�6:   
������ $
��Left=�5,   :   $	��Top=�6U   $	��136��6m   $��1��6y   #  HandyControl.Controls.TextBox  5.      -   5        
  Name$
 txtNome��5.   %   $��51,71,150,203q�66   $��1��6M   
������ $��0,0,0,1q�6s   ���	#FFFDFDFD �� 
CaretBrush
 �����$��16(�5/   �   $
��Bold"�6�   
�������6�   �� 51      -   5         ��Name$ txtbNomeplaceholder��51   $   $��Insira seu nome��6A   $
��Wrap=�52   $    J� Opacity$	 0.5Q�68   $��57,70,38,184q�53   $    J� IsHitTestVisible$ False��54   $   $��1��6=   
�������$��20(�6]   '  !MaterialDesignThemes.Wpf.PackIcon  55      -   5        $��usericon��55   2      Kind$ Account=�6B   
�������$��	-1,56,0,0q�6d   $��51��6w   $��52��6�   $��1��6�   '  !HandyControl.Controls.PasswordBox  57      -   5        $��passboxSenha��57   )      PasswordChar$ *��6=   $��52,178,144,96q�6N   $��1��6e   
������ $6��00,0,0,1                                         q�6�   
�������
�������$��16(�59   �   �� 5;      -   5        $ txtbSenhaPlaceholder��5;   $   $��Insira a senha��6B   $
��Wrap=�5<   $   $	 0.5Q�68   $��56,173,103,88q�6F   $ False��5=   $   $��1��6=   
�������$��20(�6]     5?      -	   5        $��iconKey��5?   2   $	 Key=�6A   $��1��6L    � Cursor$
 Hand��6Y   
�������$��45��6z   $��52��6�   $��	0,165,0,0q�6�   �� 5A      -
   5        $	 lblSenha��5A       $��Senha��62   $��
46,138,0,0q�5B       
������ 
�������
������ $��16(�5D       $��30��6.   $��Bahnschrift)�6:   $
��Left=�5E       $	��Top=�6;   $��1��6S   �� 5F      -   5        $��btnLogin��5F   !   $��Login��61   
���� ��$��Verdana)�5H   !   $��
156,32,0,0q�5I   !   $��5��65   )  #HandyControl.Controls.BorderElement   CornerRadius$ 15��5J   !   $
��Left=�5K   !   $	��Top=�6<   $	��284��5L   !   $��46��6-   
�������$��22(�6L   $
��Bold"�6Z   
���   �5M   8   5N      5O      �� 5P      $
��Left=�6   $ false��67   $��3,0,0,0q�6P   $
��Wrap=�6a   $��Login��6u   $	��Top=�6�   
�������$��Microsoft Sans Serif)�6�   $��18(�6�   $	��644��6�   
���   � J� OpacityMask
 �����6�   �� 5Q      -   5        $	��━��5Q      $
 Hand��6%   $��Right=�63   $��	0,0,110,0q�6O   $	��Top=�6b   $	��140��6z   $��16(�6�   $��Cascadia Code)�6�   
����� �
���   �$��41��6�   �� 5R      -   5        $	��❌��5R      $
 Hand��6%   $��Right=�63   $	��Top=�6O   $	��120��6g   $��16(�6s   $��Cascadia Code)�6�   
���  ��
���   �$��41��6�   5S      5T      5U      5V      