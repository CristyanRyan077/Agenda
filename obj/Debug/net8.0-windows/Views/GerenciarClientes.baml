   M S B A M L   `   `   `  ����E  AAgendaNovo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:AgendaNovo
AgendaNovo  5   	   "    AgendaNovo.GerenciarClientes   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:AgendaNovo     5   	   _ [MaterialDesignThemes.Wpf, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	   S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5   	   T PMahApps.Metro, Version=2.0.0.0, Culture=neutral, PublicKeyToken=51482d6f650b2b3f6mah,http://metro.mahapps.com/winfx/xaml/controls  6<     � WindowState$  	Maximized=�5
   	    � 
ResizeMode$ NoResize=�5   
    � Title$ GerenciarClientes��5   	   $	��900��6#   $
��1440��60   .��5      � .+�5   
   �� .@�5      �� $��Clientes��6   .��5      � ��5      �� 5      .��5      �  � Color$ #7be9f6��6+    � Offset$	 0.1Q�6;   � 5      $ #f093da��6+   $ 1Q�6;   5       � ColumnDefinitions 5      �� 5      $	��32* �6+   �� 5      $
��101* �6+   �� 5      $
��133* �6+   .+�5      &   System.Windows.Controls.DataGrid  $��2��6      AutoGenerateColumns$ False��5      ��5   /   �� *ListaAgendamentos+ �� SelectedItem 5      �� *NovoAgendamento+   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5       ,  &System.Windows.Controls.DataGridColumn	   Header$	 Id��61   1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 6=   �� *Id+  5!      $	 Cliente��61   
 6B   �� *Cliente.Nome+  5"      $	 Telefone��61   
 6C   �� *Cliente.Telefone+5$      #  HandyControl.Controls.TextBox  5%      -   5          Name$ 
txtCliente��5%   !   '  !HandyControl.Controls.InfoElement   Placeholder$ Cliente��65   $
��Left=�5&   !   $	��Top=�6<   $��	12,60,0,0q�5'   !    �� TextWrapping$
 Wrap=�5(   !   $	��416��5)   !   $��42��6-   
���   �$��2��6M     5*      -   5        $ txtTel��5*   !   $ Telefone��61   $
��Left=�5+      $	��Top=�64   $��
12,125,0,0q�5,      $
 Wrap=�5-      $	��416��5.      $��42��6%   
���   �$��2��6E     5/      -   5        $ txtEmail��5/   !   $ Email��63   $
��Left=�50      $	��Top=�64   $��
12,188,0,0q�51      $
 Wrap=�52      $	��416��53      $��42��6%   
���   �$��2��6E     54      -   5        $ txtId��54   !   $ ID��60   $
��Left=�55      $	��Top=�64   $��141,252,0,0q�56      $
 Wrap=�57      $	��117��58      
���   �$��46��69   $��2��6E   �� 5:      -   5         ��Name$ 	btnSalvar��5:      $��Salvar��60   ���	#FF37FF00)  #HandyControl.Controls.BorderElement   CornerRadius$ 10��5<      $
��Left=�5=      $��
16,248,0,0q�5>      $	��Top=�5?      $��50��5@      $��96��6)   
�������64   �� 5B      -   5        $ 
btnExcluir��5B      $��Excluir��61   
���  ��$ 10��5D      $
��Left=�5E      $��123,248,0,0q�5F      $	��Top=�5G      $��48��5H      $	��104��6%   
���   �$��1��6E   5J      5K      �� 5L      $��Criança��6   .��5M      � ��5N      �� 5O      .��5P      � $ #7be9f6��6+   $ 1Q�6;   � 5Q      $ #f093da��6+   $	 0.1Q�6;   5R       5T      �� 5U      $	��38* �6+   �� 5V      $
��681* �6+   �� 5W      $
��719* �6+   .+�5Y        $��2��6    5Z        5[      $	 Id��61   
 6=   �� *Id+  5]      $	 Crianca��61   
 6B   �� *Crianca.Nome+  5^      $	 Genero��61   
 6A   �� *Crianca.Genero+  5_      $	 Idade��61   
 6@   �� *Crianca.Idade+5a        5b      -   5        $ 
txtCrianca��5b   !   $ Criança��65   $
��Left=�5c       $	��Top=�6;   $��	10,51,0,0q�5d       $
 Wrap=�5e       $	��416��5f       $��42��6,   
���   �$��2��6L     5g      -   5        $ txtResponsavel��5g   !   $ Responsável��69   $
��Left=�5h      $	��Top=�63   $��
10,118,0,0q�5i      $
 Wrap=�5j      $	��416��5k      
���   �$��42��68   $��2��6D   $  HandyControl.Controls.ComboBox  5l      -	   5          Name$ 	txtGenero��5l   "   $ Gênero��65   $
��Left=�5m      $	��Top=�63   $��
10,186,0,0q�5n      $	��140��5o      
���   �$��38��68   $��2��6D     5p      -
   5        $ txtIdCrianca��5p   !   $ ID��67   $
��Left=�5q      $	��Top=�63   $��
87,248,0,0q�5r      $
 Wrap=�5s      $	��117��5t      
���   �$��1��69   $��53��6I   �� 5v      -   5        $ 
btnSalvar2��5v      $��Salvar��61   ���	#FF37FF00$ 10��5x      $
��Left=�5y      $��	7,248,0,0q�5z      $	��Top=�5{      $��56��5|      $	��105��6(   
�������$��2��6H   �� 5~      -   5        $ btnExcluir2��5~      $��Excluir��62   
���  ��$ 10��5�      $
��Left=�5�      $��219,248,0,0q�5�      $	��Top=�5�      $��60��5�      $��94��6$   
���   �$��1��6C     5�      -   5        $ txtNascimento��5�   !   $ 
Nascimento��68   $
��Left=�5�      $	��Top=�63   $��122,184,0,0q�5�      $
 Wrap=�5�      $	��269��5�      
���   �$��42��68   $��1��6D   &	   System.Windows.Controls.Calendar	  5�      -   5        $��
calendario��5�      $
��Left=�5�      $��	8,320,0,0q�6:   $y�Hidden=�6M    � Language$ pt-br�6a   $	��Top=�6r   $��2��6�    � LayoutTransform 5�      �� 5�       �� ScaleX$	 1.6Q�6-    �� ScaleY$
 1.60Q�6:   5�      5�      5�      5�      5�      5�      