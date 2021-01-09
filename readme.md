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
* 超簡易なプロキシ鯖<br>
　パケットの受け取りと、送り出しの両方に慣れておきたい。https の仕組みにも踏み込んでみると面白いと思う。
* UI 部分をブラウザに分離する<BR>
　レンタル鯖でプロキシ鯖を稼働させたときなどを想定すると、UI を遠隔地に分離できるのは便利。Websocket で簡単に実装できると思う。

## C# を知らない人はどうすれば？
　C# は簡単な言語なので、他の言語を知っていればすぐに書けるようになると思う。他の言語もやったことがなければ、以下の URL を参考にしてみては？<br>
https://github.com/kn201905/etc

---
## 以降はおまけです（読まなくても大丈夫です）

## どの言語を学べばいいですか？
* 週に１度は聞かれる質問なので、ここに書いておきます。大事なのは、どの言語「を」ではなくて、どの言語「から」と考える必要があるということ。今の時代、開発環境が無償でいくらでも手に入るし、もし今後プログラマとして働きたい、などということであれば、理解できる言語が１つではお話にならない。下の順序で C まで理解できていれば、Python、Go などの他の言語に移るのも簡単。
* 学ぶ順番は、C# → java → javascript（js）、typescript → C → 余裕があれば、C++03（いわゆる無印C++）→ さらに高みを目指すなら、C++11、C++20
* ~~C# を最初にしたのは、上の方でも述べていることだけど、非同期関数実行後の resume（await）を、複数箇所に書ける言語は C# のみであることが大きい。~~（訂正：js も C# と同じ機構を持っていました。）非同期タスクをキューの要素として扱うことができ、キューに登録したタスクの実行順序を動的に入れ替えるなどのことを、システムレベルで簡単に実現できる。そのため、現代では必須技術となっているネットワーク関連のコード開発を非常に行いやすい。
* js を最初に学ぶことを薦める人も多いけれど、スクリプトとして出発した js と、初めから言語として設計されたものを学ぶのとでは、学べることに大きな差が表れるためあまりお勧めはできない。C#、java を理解している人が書いた js のコードと、js しか知らない人が書いた js のコードを比べると、コードの質に大きな差が出ることが多い（コードの組み方の概念に大きな違いがある）。
* java は昔はお勧めできたけど、Oracle社の支配下になって以来、言語としての進化は他の言語に見劣りするようになった。例えば、現在の言語の流れでは、コルーチンをどうやって実装するかが大きな焦点になっているが、javaエンジニアに「コルーチンって何？」って聞いても分からない人が多い。java は時代の流れに取り残されつつある（やっぱり Oracle社だからなのか、、）。それでも java を２番目に取り上げたのは、C# を知っていれば java は苦労なく書けるということと、IT 系で仕事をする場合、java の案件は依然として多いためである。

## その他
* このページにあるコードは、デュラチャの荒らし対策のためのコード。以前にデュラチャの荒らし対策で話題が盛り上がったことがあり、この git リポジトリはそのときに作られたもの。ロンに荒らされて大変だった時期の名残（「readme_old.md」を参照）。ここのコードを参考にしている人がいたため、コードは残している。
* コードを共同で開発するの？と、質問されることがあるのだけど、そういう目的はない。コードを書いてみたい人がいたら、その支援はするから楽しんでみて、というのが目的。
