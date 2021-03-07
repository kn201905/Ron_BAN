## このメッセージを作った意図
　デュラチャで毎回同じ説明をするのが大変なため。

## デュラチャで立てている部屋の意図
* 仕事が忙しくて、遊びのコードを書く時間がなかなかとれない。仕事に関係のない遊びのコードは、自分の技術の幅や経験を広げるために重要なもの。
* 最近、コードを書く練習をしたいけど、書きたいコードがないという人が多いらしい。
* コードを書く練習をしたい人がいたら、技術の支援はするので、遊びのコードを書いてみてもらえるととてもありがたい。お互いに経験を広めることができたら winwin になると思う。

## 使用言語は？
　自分の母語は C++17 だけど、以下の理由で C# を選定している。
* １つの実行ファイルで、Win、Linux のどちらでも走るようにしたい
* サンデープログラマーや、初心者でも書きやすい
* java、C、C++ 等と比べて非同期メカニズムが洗練されている

## 遊びのコードってどんなもの？
* チャットシステムの構築<br>
　デュラチャのコピーを構築してみる。最近のデュラチャは荒らしが多いため、それを防止するようなシステムの構築は面白いと思う。
* TLS 1.3 の実験<br>
　サーバー側の TLS1.3 ハンドシェイクを実際に書いてみる。https の仕組みはよく知られているけど、書いてみることにより知見が深まり興味深いと思う。
* 超簡易なプロキシ鯖<br>
　利用可能な状態までは書き上げてみたが、接続数のコントロールの機能を組み込んでいない。そのため、接続先の Host に接続拒否をされることがある。<br>
　接続数コントロールのコードを書き上げることは、コード書きの練習として非常に良いと思う。
* Ron_BAN の再構築<br>
　この部屋で動作している荒らし対策のコードが Ron_BAN。短時間で適当に書き上げてしまったため、機能が少ない。もう少し機能を追加したら面白いと思う。<br>
* Windows上で動作するマークダウンサーバを .net core に移植して、Win と Linux の両方で動作できるようにすること。<br>
これは時間に余裕のあるときに自分でやろうと思っているものだけど、実用的で面白いと思っている。Win用のマークダウンサーバのソースコードは以下のURLで。  
https://github.com/kn201905/mdwiki_re

## C# を知らない人はどうすれば？
　C# は簡単な言語なので、他の言語を知っていればすぐに書けるようになると思う。他の言語もやったことがなければ、以下の URL を参考にしてみてください。<br>
https://github.com/kn201905/etc

---
## 以降はおまけです（読まなくても大丈夫です）

## どの言語を学べばいいですか？
* 週に１度は聞かれる質問なので、ここに書いておきます。大事なのは、どの言語「を」ではなくて、どの言語「から」と考える必要があるということ。今の時代、開発環境が無償でいくらでも手に入るし、もし今後プログラマとして働きたい、などということであれば、理解できる言語が１つではお話にならない。下の順序で C まで理解できていれば、Python、Go などの他の言語に移るのも簡単。
* 学ぶ順番は、C# → java → javascript（js）、typescript → C → 余裕があれば、C++03（いわゆる無印C++）→ さらに高みを目指すなら、C++11、C++20
* ~~C# を最初にしたのは、上の方でも述べていることだけど、非同期関数実行後の resume（await）を、複数箇所に書ける言語は C# のみであることが大きい。~~（訂正：js も C# と同じ機構を持っていました。）非同期タスクをキューの要素として扱うことができ、キューに登録したタスクの実行順序を動的に入れ替えるなどのことを、システムレベルで簡単に実現できる。そのため、現代では必須技術となっているネットワーク関連のコード開発を非常に行いやすい。
* js を最初に学ぶことを薦める人も多いけれど、スクリプトとして出発した js と、初めから言語として設計されたものを学ぶのとでは、学べることに大きな差が表れるためあまりお勧めはできない。C#、java を理解している人が書いた js のコードと、js しか知らない人が書いた js のコードを比べると、コードの質に大きな差が出ることが多い（コードの組み方の概念に大きな違いがある）。
* java は昔はお勧めできたけど、Oracle社の支配下になって以来、言語としての機能は他の言語に見劣りするようになった。例えば、現在の言語の流れでは、コルーチンをどうやって実装するかが大きな焦点になっているが、javaエンジニアに「コルーチンって何？」って聞いても分からない人が多い。java エンジニアは、同期といえばスレッド同期しか思いつかない人が未だに多い、、。それでも java を２番目に取り上げたのは、C# を知っていれば java は苦労なく書けるということと、IT 系で仕事をする場合、java の案件は依然として多いためである。

## その他
* このページにあるコードは、デュラチャの荒らし対策のためのコード。以前にデュラチャの荒らし対策で話題が盛り上がったことがあり、この git リポジトリはそのときに作られたもの。ロンに荒らされて大変だった時期の名残（「readme_old.md」を参照）。ここのコードを参考にしている人がいたため、コードは残している。
* コードを共同で開発するの？と、質問されることがあるのだけど、そういう目的はない。コードを書いてみたい人がいたら、その支援はするから遊んでみましょうよ、というのが目的。proxyなんて、書いてみたらすごく面白いと思うし。
* また、コードを作ったらいくらか対価がもらますか？という質問を受けるときがあるけれど、よく考えてみて欲しい。例えば proxy鯖を作ったとして、それは世界的に利用されているフリーソフトの Squid 以上の出来のもの？僕も現在利用しているけれど、Squid は言うまでもなく大変優れたソフト。フリーソフトに遠く及ばないものを作ったとして、それは対価がもらえるもの？  
proxy って動作の仕組みは想像できるけど、作ってみようとするといろんなプロトコルを知る必要があったりして、その知識は今後必ず役に立つと思うから題材に選んだということ。
