   M S B A M L   `   `   `  ����E  AAgendaNovo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:AgendaNovo
AgendaNovo  5   	   'clr-namespace:AgendaNovo
AgendaNovo  5   	   "    AgendaNovo.GerenciarClientes   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   'vmclr-namespace:AgendaNovo     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:AgendaNovo     5   	   _ [MaterialDesignThemes.Wpf, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5	   	   S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5
   	   T PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3f6mah,http://metro.mahapps.com/winfx/xaml/controls  6<     � WindowState$  	Maximized=�5   	   $	��600��6!   $	��900��60    � Title$ GerenciarClientes��5   	   $
��auto��6#   $
��auto��61   .��5      � .+�5   
   � ��5      �� 5      .��5      �  � Color$ #7be9f6��6#    � Offset$	 0.1Q�63   � 5      $ #f093da��6#   $ 1Q�63   5       � ColumnDefinitions 5      �� 5      $	��420 �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#   �� 5      $��* �6#    � RowDefinitions 5      �� 5      $��* �6    �� 5      $��* �6    �� 5       $��* �6    �� 5!      $��3* �6    �� 5"      $��4* �6    .+�5%      &   System.Windows.Controls.DataGrid  $��2��6      AutoGenerateColumns$ False��5'      $��9��66   $��3��6G   ��5%   '   �� *ListaClienteCrianca+ �� SelectedItem 5&      �� *ClienteCriancaSelecionado+ �� Mode$ TwoWay=�	   Columns	 5(      0  *System.Windows.Controls.DataGridTextColumn  5)      ,  &System.Windows.Controls.DataGridColumn
   Header$
 ID��6)   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *	ClienteId+  5*      $
 Cliente��6)    6:   �� *NomeCliente+  5+      $
 Telefone��6)    6;   �� *
Telefone+  5,      $
 Email��6)    68   �� *Email+  5-      $
 Crianca��6)    6:   �� *NomeCrianca+  5.      $
 Genero��6)    69   �� *Genero+  5/      $
 Idade��6)      
IsReadOnly$
 True��68    6J   �� *Idade+51      $  HandyControl.Controls.ComboBox  55      -   5          Name$ 
txtCliente��55      '  !HandyControl.Controls.InfoElement   Placeholder$ Cliente��6.   $
��Left=�56   !   $��Bottom=�6<   $��0��57   "    �� 
IsEditable$
 True��58   !   $��12,0,0,2q�63    �� DisplayMemberPath$
 Nome��59   !   $	��410��5<   !   $��42��6-   
���   �$��2��6M   ��5:   !   �� *ListaClientes+ �� Text 5;   !   �� *NovoCliente.Nome+5<   M   '  !MaterialDesignThemes.Wpf.PackIcon  5=      $��0��6&      Kind$ Account=�66    � Cursor$
 Hand��6E   $��40��5>   *   $��40��65   $��0��6A   $��Right=�5?   *   $��Bottom=�6F   
���   �$��0,0,9,0q�5@   =   #  HandyControl.Controls.TextBox  5C      -   5          Name$ txtTel��5C      $ Telefone��6)   $
��Left=�5D      $	��Top=�64   $��2��6L   $	��410��5F      $��42��6%   
���   �$��12,0,0,0q�6E   $��1��6W   ��5E      �� *NovoCliente.Telefone+$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�5F   W     5H      $��0��6&   $ Phone=�66   $��34��5I   *   $��34��65   $��Right=�5J   *   
���   �$��0,5,1,0q�5K   =   $��1��6N     5M      -   5        $ txtEmail��5M      $ Email��6+   $
��Left=�5N      $��Bottom=�64   $��2��6O   $��12,0,0,1q�5O      $��1��6+   $	��410��5Q      $��42��6%   
���   ���5P      �� *NovoCliente.Email+$ TwoWay=�$ PropertyChanged=�5Q   1     5S      $��0��6&   $ Email=�66   $��34��5T   *   $��34��65   $��Bottom=�6A   $��Right=�5U   *   
���   �$��0,0,0,3q�5V   =   $��1��6N     5Y      -   5        $ txtId��5Y      $ ID��6(    �� 
IsReadOnly$
 True��5[      $
��Left=�5\      $��Center=�64   $��	12,0,0,40q�5]   !   $	��117��5^      
���   �$��30��69   ��5Z      �� *NovoCliente.Id+5^   9     5`      -   5        $ 
txtCrianca��5`      $ Criança��6.   $
��Left=�5a      $	��Top=�6)   $��2��6A   $��12,2,0,0q�5b      $
 True��6    $��2��62   $
 Nome��6@   $	��410��5e      $��42��6   
���   ���5c      �� *ListaCriancasDoCliente+ 5d      �� *CriancaSelecionada.Nome+$ TwoWay=�$ PropertyChanged=�5e   &     5g      $��0��6&   $ ChildFriendly=�66   $��34��5h      $��34��6*   $
 Hand��66   $��Right=�5i      
���   �$��0,9,18,0q�5j   2   $��2��6D     5l      $��0��6   $��38��6*   $��2��66   $ Gênero��5m      $	��150��6=   $��Bottom=�5n   D   
�������
���   �$
��Left=�5o   ?   $��12(�5p      
���   �   SelectionTextBrush
 �   �$��12,0,0,1q�5q   H    5n      �� *CriancaSelecionada.Genero+.@�5s      3	  -HandyControl.Controls.AutoCompleteTextBoxItem	  $��M��6-   	  5t      $��F��6-   5u        5w      $��90��6   $��38��6$   $ Idade��60   
�������
���   �$��Center=�5y   7   $��12(�5z      $��2��6   $	��Top=�6,   
���   � �� SelectionTextBrush
 �   �$��0,49,0,0q�5{   @   ��5x   	   �� *CriancaSelecionada.Idade+$ PropertyChanged=�5{   @   �� 5}      $	��160��6   $��38��6#   $��0��6/   
�������
���   �$��Right=�5~   @   $��
#heveltica)�5      $��18(�62   $
��Bold"�6@   $��Bottom=�6R   
���   �$��2��5�   .   $��0,0,2,1q�5�   x   ��5�      �� *UnidadesIdade+ 5�      �� *!CriancaSelecionada.IdadeUnidade+$ PropertyChanged=�6x   �� 5�      $��BarreiraDagrid��6   $��1��6.   $y�Hidden=�6>   �� 5�      -   5         ��Name$ 	btnSalvar��5�      $��Salvar��6(   ���	#FF37FF00)
  #HandyControl.Controls.BorderElement 
  CornerRadius$ 10��5�      $
��Left=�5�      $��3��5�      $��28,4,0,0q�6*   $	��Top=�5�      $��50��5�      $��96��6)   
���������5�   4   �� *SalvarClienteCriancaCommand+5�   4   �� 5�      -   5        $ 
btnExcluir��5�      $��Excluir��6)   
���  ��$ 10��5�      $��0,4,28,0q�6<   $��Right=�5�      $��3��65   $��0��5�      $	��Top=�6)   $��48��5�      $	��104��6%   
���   ���5�   *   �� *+)ExcluirClienteOuCriancaSelecionadoCommand+5�   1   �� 5�      $��Editar��6   
����  �
�������$ 10��6K   $��3��6n   $��48��5�      $	��104��6!   $	��Top=�6-   $��0,4,0,0q�6�   ��6E   �� *(&EditarClienteCriancaSelecionadoCommand+6�   5�      5�      5�      