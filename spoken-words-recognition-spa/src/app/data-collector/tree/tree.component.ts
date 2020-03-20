import { Component } from '@angular/core';
import { MatTreeFlatDataSource, MatTreeFlattener } from '@angular/material/tree';
import { FlatTreeControl } from '@angular/cdk/tree';
import { NATIVE_ACCENTS } from '../../utilities/Accents';

interface TreeNode {
  name: string;
  children?: TreeNode[];
}

interface ExampleFlatNode {
  expandable: boolean;
  name: string;
  level: number;
}

@Component({
  selector: 'app-tree',
  templateUrl: './tree.component.html',
  styleUrls: ['./tree.component.scss'],
})
export class TreeComponent {

  private _transformer = (node: TreeNode, level: number) => {
    return {
      expandable: !!node.children && node.children.length > 0,
      name: node.name,
      level: level,
    };
  }

  treeControl = new FlatTreeControl<ExampleFlatNode>(
      node => node.level, node => node.expandable);

  treeFlattener = new MatTreeFlattener(
      this._transformer, node => node.level, node => node.expandable, node => node.children);

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  constructor() {
    this.dataSource.data = NATIVE_ACCENTS;
  }

  hasChild = (_: number, node: ExampleFlatNode) => node.expandable;

  set(name: string) {

    if (name != null) {
      const input = document.getElementById('accentInput') as HTMLInputElement;
      input.value = name;
      input.click();
    } else {
      const popover = document.getElementById('popover') as HTMLDivElement;
      const popoverRect = popover.getBoundingClientRect();
      const elementRect = document.activeElement.getBoundingClientRect();

      if (popoverRect.bottom - elementRect.bottom < 250) {
        popover.scrollBy(0, 250);
      }
    }

    (document.activeElement as HTMLElement).blur();
  }
}
